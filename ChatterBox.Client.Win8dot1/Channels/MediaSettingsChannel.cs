using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Media;
using ChatterBox.Client.Common.Media.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ChatterBox.Client.Win8dot1.Channels
{
    internal class MediaSettingsChannel : IMediaSettingsChannel
    {
        private VoipContext _voipContext;

        public MediaSettingsChannel(VoipContext voipContext)
        {
            _voipContext = voipContext;
        }

        public MediaDevices GetVideoCaptureDevices()
        {
            return _voipContext.GetVideoCaptureDevices();
        }

        public MediaDevices GetAudioCaptureDevices()
        {
            return _voipContext.GetAudioCaptureDevices();
        }

        public MediaDevices GetAudioPlayoutDevices()
        {
            return _voipContext.GetAudioPlayoutDevices();
        }

        public CodecInfos GetAudioCodecs()
        {
            return _voipContext.GetAudioCodecs();
        }
        public CodecInfos GetVideoCodecs()
        {
            return _voipContext.GetVideoCodecs();
        }

        public MediaDevice GetVideoDevice()
        {
            return _voipContext.GetVideoDevice();
        }
        public void SetVideoDevice(MediaDevice device)
        {
            _voipContext.SetVideoDevice(device);
        }

        public MediaDevice GetAudioDevice()
        {
            return _voipContext.GetAudioDevice();
        }
        public void SetAudioDevice(MediaDevice device)
        {
            _voipContext.SetAudioDevice(device);
        }

        public CodecInfo GetVideoCodec()
        {
            return _voipContext.GetVideoCodec();
        }
        public void SetVideoCodec(CodecInfo codec)
        {
            _voipContext.SetVideoCodec(codec);
        }

        public CodecInfo GetAudioCodec()
        {
            return _voipContext.GetAudioCodec();
        }
        public void SetAudioCodec(CodecInfo codec)
        {
            _voipContext.SetAudioCodec(codec);
        }

        public MediaDevice GetAudioPlayoutDevice()
        {
            return _voipContext.GetAudioPlayoutDevice();
        }
        public void SetAudioPlayoutDevice(MediaDevice device)
        {
            _voipContext.SetAudioPlayoutDevice(device);
        }

        public CaptureCapabilities GetVideoCaptureCapabilities(MediaDevice device)
        {
            return _voipContext.GetVideoCaptureCapabilities(device).Result;
        }
        public IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device)
        {
            return _voipContext.GetVideoCaptureCapabilities(device).AsAsyncOperation();
        }

        public void SetPreferredVideoCaptureFormat(int width, int height, int frameRate)
        {
            _voipContext.SetPreferredVideoCaptureFormat(width, height, frameRate);
        }

        public void SetPreferredVideoCaptureFormat(VideoCaptureFormat format)
        {
            _voipContext.SetPreferredVideoCaptureFormat(format);
        }

        void IMediaSettingsChannel.InitializeMedia()
        {
            _voipContext.InitializeMediaAsync().Wait();
        }
        IAsyncAction IMediaSettingsChannel.InitializeMediaAsync()
        {
            return _voipContext.InitializeMediaAsync().AsAsyncAction();
        }

        public void StartTrace()
        {
            _voipContext.StartTrace();
        }

        public void StopTrace()
        {
            _voipContext.StopTrace();
        }
        public void SaveTrace(string ip, int port)
        {
            _voipContext.SaveTrace(ip, port);
        }

        public void SaveTrace(TraceServerConfig traceServer)
        {
            _voipContext.SaveTrace(traceServer);
        }

        public void ReleaseDevices()
        {
            _voipContext.ReleaseDevices();
        }
    }
}
