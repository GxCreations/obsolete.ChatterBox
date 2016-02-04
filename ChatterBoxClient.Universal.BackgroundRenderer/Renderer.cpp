#include <mfapi.h>
#include <mfidl.h>
#include "Renderer.h"
#include "MediaEngineNotify.h"

using namespace ChatterBoxClient::Universal::BackgroundRenderer;
using namespace Platform;
using Microsoft::WRL::Wrappers::HStringReference;
using Microsoft::WRL::ComPtr;
using Microsoft::WRL::Details::Make;
using ABI::Windows::Foundation::Collections::IMap;
using ABI::Windows::Foundation::Collections::IPropertySet;

Renderer::Renderer() :
    _useHardware(true),
    _foregroundProcessId(0),
    _staleHandleTimestamp(0LL),
    _gpuVideoBuffersSupported(false)
{
    InitializeCriticalSection(&_lock);
}

Renderer::~Renderer()
{
  OutputDebugString(L"Renderer::~Renderer()\n");
  Teardown();
  DeleteCriticalSection(&_lock);
}

void Renderer::Teardown() {
  OutputDebugString(L"Renderer::Teardown()\n");
  if (_mediaEngine != nullptr) {
    OutputDebugString(L"_mediaEngine->Shutdown()\n");
    _mediaEngine->Shutdown();
    _mediaEngine.Reset();
  }
  _mediaEngineEx.Reset();
  _mediaExtensionManager.Reset();
  _extensionManagerProperties.Reset();
  _device.Reset();
  _dx11DeviceContext.Reset();
  _dxGIManager.Reset();
  _swapChainHandle.Close();

  _streamSource = nullptr;
}

void Renderer::SetupRenderer(uint32 foregroundProcessId, Windows::Media::Core::IMediaSource^ streamSource,
    Windows::Foundation::Size videoControlSize)
{
    OutputDebugString(L"Renderer::SetupRenderer\n");
    _renderControlSize = videoControlSize;
    _streamSource = streamSource;
    _foregroundProcessId = foregroundProcessId;
    SetupSchemeHandler();
    SetupDirectX();
    boolean replaced;
    auto streamInspect = reinterpret_cast<IInspectable*>(streamSource);
    std::wstring url(L"webrtc://");
    GUID result;
    HRESULT hr = CoCreateGuid(&result);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to create a GUID"));
    }
    Guid gd(result);
    url += gd.ToString()->Data();
    hr = _extensionManagerProperties->Insert(HStringReference(url.c_str()).Get(), streamInspect, &replaced);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to insert a media stream into media properties"));
    }
    BSTR sourceBSTR;
    sourceBSTR = SysAllocString(url.c_str());
    hr = _mediaEngine->SetSource(sourceBSTR);
    SysFreeString(sourceBSTR);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to set media source"));
    }
    hr = _mediaEngine->Load();
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed load media from source"));
    }
}

void Renderer::SetRenderControlSize(Windows::Foundation::Size size)
{
    EnterCriticalSection(&_lock);
    _renderControlSize = size;
    AsyncRecalculateScale();
    LeaveCriticalSection(&_lock);
}

bool Renderer::GPUVideoBuffersSupported::get()
{
    return _gpuVideoBuffersSupported;
}

uint32 Renderer::GetProcessId()
{
    return ::GetCurrentProcessId();
}

void Renderer::OnMediaEngineEvent(uint32 meEvent, uintptr_t param1, uint32 param2)
{
    HANDLE swapChainHandle;
    switch ((DWORD)meEvent)
    {
    case MF_MEDIA_ENGINE_EVENT_ERROR:
        throw ref new COMException((HRESULT)param2, ref new String(L"Failed OnMediaEngineEvent"));
        break;
    case MF_MEDIA_ENGINE_EVENT_FORMATCHANGE:
        ReleaseStaleSwapChainHandleWhenExpired();
        if ((SUCCEEDED(_mediaEngineEx->GetVideoSwapchainHandle(&swapChainHandle))) &&
            (swapChainHandle != nullptr) && (swapChainHandle != INVALID_HANDLE_VALUE))
        {
            SendSwapChainHandle(swapChainHandle);
        }
        break;
    case MF_MEDIA_ENGINE_EVENT_CANPLAY:
        _mediaEngine->Play();
        break;
    case MF_MEDIA_ENGINE_EVENT_TIMEUPDATE:
        ReleaseStaleSwapChainHandleWhenExpired();
        break;
    }
}

void Renderer::SetupSchemeHandler()
{
    using Windows::Foundation::ActivateInstance;
    HRESULT hr = ActivateInstance(HStringReference(RuntimeClass_Windows_Media_MediaExtensionManager).Get(),
        _mediaExtensionManager.ReleaseAndGetAddressOf());
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to create media extension manager"));
    }
    ComPtr<IMap<HSTRING, IInspectable*>> props;
    hr = ActivateInstance(HStringReference(RuntimeClass_Windows_Foundation_Collections_PropertySet).Get(),
        props.ReleaseAndGetAddressOf());
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to create collection property set"));
    }
    ComPtr<IPropertySet> propSet;
    props.As(&propSet);
    HStringReference clsid(L"ChatterBoxClient.Universal.BackgroundRenderer.SchemeHandler");
    HStringReference scheme(L"webrtc:");
    hr = _mediaExtensionManager->RegisterSchemeHandlerWithSettings(clsid.Get(), scheme.Get(), propSet.Get());
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to to register scheme handler"));
    }
    _extensionManagerProperties = props;
}

void Renderer::SetupDirectX()
{
    _useHardware = true;
    HRESULT hr = MFStartup(MF_VERSION);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"MFStartup failed"));
    }
    CreateDXDevice();
    UINT resetToken;
    hr = MFCreateDXGIDeviceManager(&resetToken, &_dxGIManager);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"MFCreateDXGIDeviceManager failed"));
    }
    hr = _dxGIManager->ResetDevice(_device.Get(), resetToken);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"ResetDevice failed"));
    }
    ComPtr<IMFMediaEngineClassFactory> factory;
    hr = CoCreateInstance(CLSID_MFMediaEngineClassFactory, nullptr, CLSCTX_ALL, IID_PPV_ARGS(&factory));
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to create media engine class factory"));
    }
    ComPtr<IMFAttributes> attributes;
    hr = MFCreateAttributes(&attributes, 3);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"MFCreateAttributes failed"));
    }
    hr = attributes->SetUnknown(MF_MEDIA_ENGINE_DXGI_MANAGER, (IUnknown*)_dxGIManager.Get());
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to set the DXGI manager"));
    }
    ComPtr<MediaEngineNotify> notify;
    notify = Make<MediaEngineNotify>();
    notify->SetCallback(this);
    hr = attributes->SetUINT32(MF_MEDIA_ENGINE_VIDEO_OUTPUT_FORMAT, DXGI_FORMAT_NV12);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"attributes->SetUINT32(MF_MEDIA_ENGINE_VIDEO_OUTPUT_FORMAT, DXGI_FORMAT_NV12) failed"));
    }
    hr = attributes->SetUINT32(MF_MEDIA_ENGINE_VIDEO_OUTPUT_FORMAT, DXGI_SCALING_STRETCH);
    hr = attributes->SetUnknown(MF_MEDIA_ENGINE_CALLBACK, (IUnknown*)notify.Get());
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"attributes->SetUnknown(MF_MEDIA_ENGINE_CALLBACK, (IUnknown*)notify.Get()) failed"));
    }
    hr = factory->CreateInstance(
        MF_MEDIA_ENGINE_REAL_TIME_MODE | MF_MEDIA_ENGINE_WAITFORSTABLE_STATE,
        attributes.Get(), &_mediaEngine);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to create media engine"));
    }
    hr = _mediaEngine.As(&_mediaEngineEx);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to create media engineex"));
    }
    hr = _mediaEngineEx->EnableWindowlessSwapchainMode(TRUE);
    if (FAILED(hr))
    {
        throw ref new COMException(hr, ref new String(L"Failed to enable Windowsless swapchain mode"));
    }
    _mediaEngineEx->SetRealTimeMode(TRUE);
}

void Renderer::CreateDXDevice()
{
    static const D3D_FEATURE_LEVEL levels[] =
    {
        D3D_FEATURE_LEVEL_11_1,
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0,
        D3D_FEATURE_LEVEL_9_1,
        D3D_FEATURE_LEVEL_9_2,
        D3D_FEATURE_LEVEL_9_3
    };

    D3D_FEATURE_LEVEL featureLevel;
    HRESULT hr = S_OK;
    _gpuVideoBuffersSupported = false;

    if (_useHardware)
    {
        hr = D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr,
            D3D11_CREATE_DEVICE_VIDEO_SUPPORT,
            levels, ARRAYSIZE(levels), D3D11_SDK_VERSION, &_device, &featureLevel,
            &_dx11DeviceContext);
    }

    if (FAILED(hr))
    {
        _useHardware = false;
    }

    if (!_useHardware)
    {
        hr = D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_WARP, nullptr,
            D3D11_CREATE_DEVICE_VIDEO_SUPPORT, levels, ARRAYSIZE(levels),
            D3D11_SDK_VERSION, &_device, &featureLevel, &_dx11DeviceContext);
        if (FAILED(hr))
        {
            throw ref new COMException(hr, ref new String(L"Failed to create a DX device"));
        }
    }

    if (_useHardware)
    {
        if ((unsigned int)featureLevel >= (unsigned int)D3D_FEATURE_LEVEL_11_1)
        {
            _gpuVideoBuffersSupported = true;
        }
        ComPtr<ID3D10Multithread> multithread;
        hr = _device.Get()->QueryInterface(IID_PPV_ARGS(&multithread));
        if (FAILED(hr))
        {
            throw ref new COMException(hr, ref new String(L"Failed to set device to multithreaded"));
        }
        multithread->SetMultithreadProtected(TRUE);
    }
}

void Renderer::SendSwapChainHandle(HANDLE swapChain)
{
    _swapChainHandle.DetachMove(_staleSwapChainHandle);
    _staleHandleTimestamp = GetTickCount64();
    _swapChainHandle.AssignHandle(swapChain, _foregroundProcessId);
    DWORD width;
    DWORD height;
    _mediaEngine->GetNativeVideoSize(&width, &height);

    OutputDebugString(L"RenderFormatUpdate\n");
    RenderFormatUpdate((int64)_swapChainHandle.GetRemoteHandle(), width, height);
    Windows::Foundation::Size size;
    size.Width = (float)width;
    size.Height = (float)height;
    EnterCriticalSection(&_lock);
    _videoSize = size;
    AsyncRecalculateScale();
    LeaveCriticalSection(&_lock);
}

void Renderer::AsyncRecalculateScale()
{
    EnterCriticalSection(&_lock);
    Windows::Foundation::Size renderControlSize = _renderControlSize;
    Windows::Foundation::Size videoSize = _videoSize;
    LeaveCriticalSection(&_lock);
    concurrency::create_async([this, renderControlSize, videoSize]
    {
        RecalculateScale(renderControlSize, videoSize);
    });
}

void Renderer::RecalculateScale(Windows::Foundation::Size renderControlSize,
    Windows::Foundation::Size videoSize)
{
    if ((renderControlSize.Width <= 0.0f) || (renderControlSize.Height <= 0.0f) ||
        (videoSize.Width <= 0.0f) || (videoSize.Height <= 0.0f))
    {
        return;
    }
    if (_mediaEngineEx == nullptr)
    {
        return;
    }
    MFVideoNormalizedRect rect = MFVideoNormalizedRect{ 0.0f, 0.0f, 0.0f, 0.0f };
    double videoAspect = double(videoSize.Width) / double(videoSize.Height);
    double renderControlAspect = double(renderControlSize.Width) / double(renderControlSize.Height);
    double scalingFactor = (videoAspect > renderControlAspect) ?
        (double(renderControlSize.Height) / double(videoSize.Height)) :
        (double(renderControlSize.Width) / double(videoSize.Width));
    if (scalingFactor == 0.0)
    {
        scalingFactor = 0.0001;
    }
    int scaledRenderControlWidth = int(double(renderControlSize.Width) / scalingFactor);
    int scaledRenderControlHeight = int(double(renderControlSize.Height) / scalingFactor);
    int cropX = max((int)videoSize.Width - scaledRenderControlWidth, 0);
    int cropY = max((int)videoSize.Height - scaledRenderControlHeight, 0);
    float cropFrX = float((double(cropX) / double(videoSize.Width)) / 2.0);
    float cropFrY = float((double(cropY) / double(videoSize.Height)) / 2.0);
    rect = MFVideoNormalizedRect{ cropFrX, cropFrY, 1.0f - cropFrX, 1.0f - cropFrY };
    RECT r = { 0, 0, (LONG)renderControlSize.Width , (LONG)renderControlSize.Height };
    MFARGB borderColour = { 0 };
    _mediaEngineEx->UpdateVideoStream(&rect, &r, &borderColour);
}

void Renderer::ReleaseStaleSwapChainHandle()
{
    _staleSwapChainHandle.Close();
    _staleHandleTimestamp = 0;
}

void Renderer::ReleaseStaleSwapChainHandleWhenExpired()
{
    if (!_staleSwapChainHandle.IsValid())
    {
        return;
    }
    ULONGLONG currentTime = GetTickCount64();
    if ((currentTime - _staleHandleTimestamp) >= StaleHandleTimeoutMS)
    {
        ReleaseStaleSwapChainHandle();
    }
}