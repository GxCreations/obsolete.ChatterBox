using ChatterBox.Client.Common.Background;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Foreground.Dto.ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Media;
using ChatterBox.Client.Common.Media.Dto;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Universal.Background.DeferralWrappers;
using ChatterBox.Client.Universal.Background.Helpers;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Universal.Services
{
    public class HubClient : DispatcherBindableBase,
        IForegroundUpdateService,
        ISignalingSocketChannel,
        IClientChannel,
        IVoipChannel,
        IForegroundChannel,
        IMediaSettingsChannel
    {
        private readonly TaskHelper _taskHelper;
        private AppServiceConnection _appConnection;
        private ApplicationDataContainer _localSettings;

        public HubClient(CoreDispatcher uiDispatcher, TaskHelper taskHelper) : base(uiDispatcher)
        {
            _taskHelper = taskHelper;
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public bool IsConnected { get; private set; }

        #region IMediaSettingsChannel

        //private Media _media;

        public MediaDevices GetVideoCaptureDevices()
        {
            return InvokeHubChannel<IMediaSettingsChannel, MediaDevices>();
        }

        public MediaDevices GetAudioCaptureDevices()
        {
            return InvokeHubChannel<IMediaSettingsChannel, MediaDevices>();
        }

        public MediaDevices GetAudioPlayoutDevices()
        {
            return InvokeHubChannel<IMediaSettingsChannel, MediaDevices>();
        }

        public CodecInfos GetAudioCodecs()
        {
            return InvokeHubChannel<IMediaSettingsChannel, CodecInfos>();
        }

        public CodecInfos GetVideoCodecs()
        {
            return InvokeHubChannel<IMediaSettingsChannel, CodecInfos>();
        }

        public MediaDevice GetVideoDevice()
        {
            return InvokeHubChannel<IMediaSettingsChannel, MediaDevice>();
        }
        public void SetVideoDevice(MediaDevice device)
        {
            InvokeHubChannel<IMediaSettingsChannel>(device);
        }

        public MediaDevice GetAudioDevice()
        {
            return InvokeHubChannel<IMediaSettingsChannel, MediaDevice>();
        }
        public void SetAudioDevice(MediaDevice device)
        {
            InvokeHubChannel<IMediaSettingsChannel>(device);
        }

        public CodecInfo GetVideoCodec()
        {
            return InvokeHubChannel<IMediaSettingsChannel, CodecInfo>();
        }
        public void SetVideoCodec(CodecInfo codec)
        {
            InvokeHubChannel<IMediaSettingsChannel>(codec);
        }

        public CodecInfo GetAudioCodec()
        {
            return InvokeHubChannel<IMediaSettingsChannel, CodecInfo>();
        }
        public void SetAudioCodec(CodecInfo codec)
        {
            InvokeHubChannel<IMediaSettingsChannel>(codec);
        }

        public MediaDevice GetAudioPlayoutDevice()
        {
            return InvokeHubChannel<IMediaSettingsChannel, MediaDevice>();
        }
        public void SetAudioPlayoutDevice(MediaDevice device)
        {
            InvokeHubChannel<IMediaSettingsChannel>(device);
        }

        public CaptureCapabilities GetVideoCaptureCapabilities(MediaDevice device)
        {
            return InvokeHubChannel<IMediaSettingsChannel, CaptureCapabilities>(device);
        }
        public IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device)
        {
            return Task.Run(async () =>
            {
                var settings = new MediaCaptureInitializationSettings()
                {
                    VideoDeviceId = device.Id,
                    MediaCategory = MediaCategory.Communications,
                };
                var capture = new MediaCapture();
                await capture.InitializeAsync(settings);
                var caps = capture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);

                var arr = new List<CaptureCapability>();
                foreach (var cap in caps)
                {
                    if (cap.Type != "Video")
                    {
                        continue;
                    }

                    var videoCap = (VideoEncodingProperties)cap;

                    if (videoCap.FrameRate.Denominator == 0 ||
                    videoCap.FrameRate.Numerator == 0 ||
                    videoCap.Width == 0 ||
                    videoCap.Height == 0)
                    {
                        continue;
                    }
                    var captureCap = new CaptureCapability()
                    {
                        Width = videoCap.Width,
                        Height = videoCap.Height,
                        FrameRate = videoCap.FrameRate.Numerator / videoCap.FrameRate.Denominator,
                    };
                    captureCap.FrameRateDescription = $"{captureCap.FrameRate} fps";
                    captureCap.ResolutionDescription = $"{captureCap.Width} x {captureCap.Height}";
                    captureCap.PixelAspectRatio = new Common.Media.Dto.MediaRatio()
                    {
                        Numerator = videoCap.PixelAspectRatio.Numerator,
                        Denominator = videoCap.PixelAspectRatio.Denominator,
                    };
                    captureCap.FullDescription = $"{captureCap.ResolutionDescription} {captureCap.FrameRateDescription}";
                    arr.Add(captureCap);
                }
                return new CaptureCapabilities() { Capabilities = arr.GroupBy(o => o.FullDescription).Select(o => o.First()).ToArray() };
            }).AsAsyncOperation();
        }

        public void SetPreferredVideoCaptureFormat(VideoCaptureFormat format)
        {
            InvokeHubChannel<IMediaSettingsChannel>(format);
        }

        public void InitializeMedia()
        {
            InvokeHubChannel<IMediaSettingsChannel>();
        }
        public IAsyncAction InitializeMediaAsync()
        {
            return Task.Run(() => InitializeMedia()).AsAsyncAction();
        }

        public IAsyncOperation<bool> RequestAccessForMediaCaptureAsync()
        {
            // do not call for Windows 10
            throw new System.NotSupportedException();
        }

        public void SyncWithNTP(long ntpTime)
        {
            InvokeHubChannel<IMediaSettingsChannel>(ntpTime);
        }
        public void StartTrace()
        {
          InvokeHubChannel<IMediaSettingsChannel>();
        }

        public void StopTrace()
        {
           InvokeHubChannel<IMediaSettingsChannel>();
        }

        public void SaveTrace(TraceServerConfig traceServer)
        {
          InvokeHubChannel<IMediaSettingsChannel>(traceServer);
        }

        public void ReleaseDevices()
        {
            InvokeHubChannel<IMediaSettingsChannel>();
        }

        #endregion

        #region IClientChannel Members

        public void ClientConfirmation(Confirmation confirmation)
        {
            InvokeHubChannel<IClientChannel>(confirmation);
        }

        public void ClientHeartBeat()
        {
            InvokeHubChannel<IClientChannel>();
        }

        public void GetPeerList(Message message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        public void Register(Registration message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        public void Relay(RelayMessage message)
        {
            InvokeHubChannel<IClientChannel>(message);
        }

        #endregion

        #region IForegroundChannel Members

        public void OnSignaledPeerDataUpdated()
        {
            RunOnUiThread(() => OnPeerDataUpdated?.Invoke());
        }

        public void OnSignaledRegistrationStatusUpdated()
        {
            RunOnUiThread(() => OnRegistrationStatusUpdated?.Invoke());
        }

        public void OnSignaledRelayMessagesUpdated()
        {
            RunOnUiThread(() => OnRelayMessagesUpdated?.Invoke());
        }

        public void OnVoipState(VoipState state)
        {
            RunOnUiThread(() => OnVoipStateUpdate?.Invoke(state));
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
            RunOnUiThread(() => OnFrameFormatUpdate?.Invoke(frameFormat));
        }

        public ForegroundState GetForegroundState()
        {
            return new ForegroundState { IsForegroundVisible = true };
        }

        public string GetShownUserId()
        {
            if (GetShownUser != null)
                return GetShownUser();
            return string.Empty;
        }

        #endregion

        #region IForegroundUpdateService Members

        public event Action OnPeerDataUpdated;
        public event Action OnRegistrationStatusUpdated;
        public event Action OnRelayMessagesUpdated;
        public event Action<VoipState> OnVoipStateUpdate;
        public event Action<FrameFormat> OnFrameFormatUpdate;
        public event Func<string> GetShownUser;

        #endregion

        #region ISignalingSocketChannel Members

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            return InvokeHubChannel<ISignalingSocketChannel, ConnectionStatus>(new ConnectionOwner
            {
                OwnerId = _taskHelper.GetTask(nameof(SignalingTask)).TaskId.ToString()
            });
        }
        public void DisconnectSignalingServer()
        {
            InvokeHubChannel<ISignalingSocketChannel>();
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return InvokeHubChannel<ISignalingSocketChannel, ConnectionStatus>();
        }

        #endregion

        #region IVoipChannel Members

        public void SetForegroundProcessId(uint processId)
        {
            InvokeHubChannel<IVoipChannel>(processId);
        }

        public void Answer()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void Call(OutgoingCallRequest request)
        {
            InvokeHubChannel<IVoipChannel>(request);
        }

        public VoipState GetVoipState()
        {
            return InvokeHubChannel<IVoipChannel, VoipState>();
        }

        public void Hangup()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void OnIceCandidate(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnIncomingCall(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOutgoingCallAccepted(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOutgoingCallRejected(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnRemoteHangup(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnAnswer(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void OnOffer(RelayMessage message)
        {
            InvokeHubChannel<IVoipChannel>(message);
        }

        public void Reject(IncomingCallReject reason)
        {
            InvokeHubChannel<IVoipChannel>(reason);
        }

        public void DisplayOrientationChanged(DisplayOrientations orientation)
        {
            InvokeHubChannel<IVoipChannel>(orientation);
        }

        public void ConfigureMicrophone(MicrophoneConfig config)
        {
            InvokeHubChannel<IVoipChannel>(config);
        }

        public void SuspendVoipVideo()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void ResumeVoipVideo()
        {
            InvokeHubChannel<IVoipChannel>();
        }

        public void ConfigureVideo(VideoConfig config)
        {
            InvokeHubChannel<IVoipChannel>(config);
        }

        public void OnRemoteControlSize(Windows.Foundation.Size size)
        {
            InvokeHubChannel<IVoipChannel>(size);
        }
        public void OnLocalControlSize(Windows.Foundation.Size size)
        {
            InvokeHubChannel<IVoipChannel>(size);
        }
        #endregion

        public async Task<bool> Connect()
        {
            _appConnection = new AppServiceConnection
            {
                AppServiceName = nameof(ForegroundAppServiceTask),
                PackageFamilyName = Package.Current.Id.FamilyName
            };
            _appConnection.ServiceClosed += OnServiceClosed;
            _appConnection.RequestReceived += OnRequestReceived;
            var status = await _appConnection.OpenAsync();
            IsConnected = (status == AppServiceConnectionStatus.Success);
            return IsConnected;
        }

        private void InvokeHubChannel<TContract>(object arg = null, [CallerMemberName] string method = null)
        {
            _appConnection.InvokeChannel(typeof(TContract), arg, method);
        }

        private TResult InvokeHubChannel<TContract, TResult>(object arg = null, [CallerMemberName] string method = null)
            where TResult : class
        {
            return (TResult)_appConnection.InvokeChannel(typeof(TContract), arg, method, typeof(TResult));
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            using (new AppServiceDeferralWrapper(args.GetDeferral()))
            {
                var message = args.Request.Message.Single().Value.ToString();
                AppServiceChannelHelper.HandleRequest(args.Request, this, message);
            }
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            IsConnected = false;
            Debug.WriteLine("HubClient.OnServiceClosed()");
            OnDisconnectedFromHub?.Invoke();
        }

        public void OnSignaledDataUpdated()
        {
            RunOnUiThread(() => OnUpdate?.Invoke());
        }

        public void RegisterVideoElements(MediaElement self, MediaElement peer)
        {
        }

        public event Action OnUpdate;


        public event Action OnDisconnectedFromHub;
    }
}