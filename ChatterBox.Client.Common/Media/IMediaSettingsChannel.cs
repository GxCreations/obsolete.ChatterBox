using System.Collections.Generic;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Media.Dto;
using Windows.Foundation;

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

        void SetPreferredVideoCaptureFormat(int width, int height, int frameRate);

        // TODO: Remove this and auto-intialize RTC
        void InitializeMedia();
        IAsyncAction InitializeMediaAsync();

        void StartTrace();
        void StopTrace();
        void SaveTrace(string ip, int port);

        void ReleaseDevices();
    }
}
