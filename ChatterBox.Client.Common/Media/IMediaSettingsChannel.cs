using System.Collections.Generic;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Media.Dto;
using Windows.Foundation;
using ChatterBox.Client.Common.Communication.Voip.Dto;

namespace ChatterBox.Client.Common.Media
{
    public interface IMediaSettingsChannel
    {
        MediaDevices GetVideoCaptureDevices();

        MediaDevices GetAudioCaptureDevices();

        MediaDevices GetAudioPlayoutDevices();

        CodecInfos GetAudioCodecs();

        CodecInfos GetVideoCodecs();

        MediaDevice GetVideoDevice();
        void SetVideoDevice(MediaDevice device);

        MediaDevice GetAudioDevice();
        void SetAudioDevice(MediaDevice device);

        CodecInfo GetVideoCodec();
        void SetVideoCodec(CodecInfo codec);

        CodecInfo GetAudioCodec();
        void SetAudioCodec(CodecInfo codec);

        MediaDevice GetAudioPlayoutDevice();
        void SetAudioPlayoutDevice(MediaDevice device);

        CaptureCapabilities GetVideoCaptureCapabilities(MediaDevice device);
        IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device);

        void SetPreferredVideoCaptureFormat(VideoCaptureFormat format);

        // TODO: Remove this and auto-intialize RTC
        void InitializeMedia();
        IAsyncAction InitializeMediaAsync();

        IAsyncOperation<bool> RequestAccessForMediaCaptureAsync();

        void SyncWithNTP(long ntpTime);
        void StartTrace();
        void StopTrace();
        void SaveTrace(TraceServerConfig config);

        void ReleaseDevices();
    }
}
