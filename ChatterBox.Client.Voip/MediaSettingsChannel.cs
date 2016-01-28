//using System;
//using System.Diagnostics;
//using Windows.Graphics.Display;
//using ChatterBox.Client.Common.Communication.Foreground.Dto;
//using ChatterBox.Client.Common.Communication.Voip.Dto;
//using ChatterBox.Common.Communication.Messages.Relay;
//using Windows.UI.Core;
//using Windows.UI.Xaml.Controls;
//using ChatterBox.Client.Voip;
//using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Media;
using ChatterBox.Client.Common.Media.Dto;
using ChatterBox.Client.Voip;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class MediaSettingsChannel : IMediaSettingsChannel
    {
        private readonly IHub _hub;

        // This variable should not be used outside of the getter below.

        private CoreDispatcher Dispatcher { get; }

        private VoipContext Context { get; }

        public MediaSettingsChannel(IHub hub, CoreDispatcher dispatcher, VoipContext context)
        {
            _hub = hub;
            Dispatcher = dispatcher;
            Context = context;
        }

        #region IMediaSettingsChannel Members

        public MediaDevices GetVideoCaptureDevices()
        {
            return Context.GetVideoCaptureDevices();
        }

        public MediaDevices GetAudioCaptureDevices()
        {
            return Context.GetAudioCaptureDevices();
        }

        public MediaDevices GetAudioPlayoutDevices()
        {
            return Context.GetAudioPlayoutDevices();
        }

        public CodecInfos GetAudioCodecs()
        {
            return Context.GetAudioCodecs();
        }

        public CodecInfos GetVideoCodecs()
        {
            return Context.GetVideoCodecs();
        }

        public MediaDevice GetVideoDevice()
        {
            return Context.GetVideoDevice();
        }
        public void SetVideoDevice(MediaDevice device)
        {
            Context.SetVideoDevice(device);
        }

        public MediaDevice GetAudioDevice()
        {
            return Context.GetAudioDevice();
        }
        public void SetAudioDevice(MediaDevice device)
        {
            Context.SetAudioDevice(device);
        }

        public CodecInfo GetVideoCodec()
        {
            return Context.GetVideoCodec();
        }
        public void SetVideoCodec(CodecInfo codec)
        {
            Context.SetVideoCodec(codec);
        }

        public CodecInfo GetAudioCodec()
        {
            return Context.GetAudioCodec();
        }
        public void SetAudioCodec(CodecInfo codec)
        {
            Context.SetAudioCodec(codec);
        }


        public MediaDevice GetAudioPlayoutDevice()
        {
            return Context.GetAudioPlayoutDevice();
        }
        public void SetAudioPlayoutDevice(MediaDevice device)
        {
            Context.SetAudioPlayoutDevice(device);
        }

        public CaptureCapabilities GetVideoCaptureCapabilities(MediaDevice device)
        {
            return Task.Run< CaptureCapabilities >(() => Context.GetVideoCaptureCapabilities(device).Result).Result;
        }

        public IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device)
        {
            return Context.GetVideoCaptureCapabilities(device).AsAsyncOperation();
        }

        public void SetPreferredVideoCaptureFormat(int width, int height, int frameRate)
        {
            Context.SetPreferredVideoCaptureFormat(new VideoCaptureFormat(width, height, frameRate));
        }
        public void SetPreferredVideoCaptureFormat(VideoCaptureFormat format)
        {
            Context.SetPreferredVideoCaptureFormat(format);
        }

        public void InitializeMedia()
        {
            // TODO please remove this method and auto-init WebRTC as needed (Fred LaBel)
            Task.Run(() => { Context.InitializeMediaAsync().Wait(); }).Wait();
        }

        public IAsyncAction InitializeMediaAsync()
        {
            // TODO please remove this method and auto-init WebRTC as needed (Fred LaBel)
            return Context.InitializeMediaAsync().AsAsyncAction();
        }

        public void StartTrace()
        {
            Context.StartTrace();
        }

        public void StopTrace()
        {
            Context.StopTrace();
        }

        public void SaveTrace(string ip, int port)
        {
            TraceServerConfig traceServer = new TraceServerConfig
            {
                Ip = ip,
                Port = port
            };
            SaveTrace(traceServer);
        }

        public void SaveTrace(TraceServerConfig traceServer)
        {
            Context.SaveTrace(traceServer);
        }

        public void ReleaseDevices()
        {
            Context.ReleaseDevices();
        }

        #endregion
    }
}
