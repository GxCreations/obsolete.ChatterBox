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
            return Task.Run(() => {
                return Context.WithContextFunc<MediaDevices>(ctx => { return ctx.GetVideoCaptureDevices(); });
            }).Result;
        }

        public MediaDevices GetAudioCaptureDevices()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<MediaDevices>(ctx => { return ctx.GetAudioCaptureDevices(); });
            }).Result;
        }

        public MediaDevices GetAudioPlayoutDevices()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<MediaDevices>(ctx => { return ctx.GetAudioPlayoutDevices(); });
            }).Result;
        }

        public CodecInfos GetAudioCodecs()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<CodecInfos>(ctx => { return ctx.GetAudioCodecs(); });
            }).Result;
        }

        public CodecInfos GetVideoCodecs()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<CodecInfos>(ctx => { return ctx.GetVideoCodecs(); });
            }).Result;
        }

        public MediaDevice GetVideoDevice()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<MediaDevice>(ctx => { return ctx.GetVideoDevice(); });
            }).Result;
        }
        public void SetVideoDevice(MediaDevice device)
        {
            Task.Run(async () => {
                await Context.WithContextAction<MediaDevice>((ctx, value) => { ctx.SetVideoDevice(value); }, device);
            }).Wait();
        }

        public MediaDevice GetAudioDevice()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<MediaDevice>(ctx => { return ctx.GetAudioDevice(); });
            }).Result;
        }
        public void SetAudioDevice(MediaDevice device)
        {
            Task.Run(async () => {
                await Context.WithContextAction<MediaDevice>((ctx, value) => { ctx.SetAudioDevice(value); }, device);
            }).Wait();
        }

        public CodecInfo GetVideoCodec()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<CodecInfo>(ctx => { return ctx.GetVideoCodec(); });
            }).Result;
        }
        public void SetVideoCodec(CodecInfo codec)
        {
            Task.Run(async () => {
                await Context.WithContextAction<CodecInfo>((ctx, value) => { ctx.SetVideoCodec(value); }, codec);
            }).Wait();
        }

        public CodecInfo GetAudioCodec()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<CodecInfo>(ctx => { return ctx.GetAudioCodec(); });
            }).Result;
        }
        public void SetAudioCodec(CodecInfo codec)
        {
            Task.Run(async () => {
                await Context.WithContextAction<CodecInfo>((ctx, value) => { ctx.SetAudioCodec(value); }, codec);
            }).Wait();
        }


        public MediaDevice GetAudioPlayoutDevice()
        {
            return Task.Run(() => {
                return Context.WithContextFunc<MediaDevice>(ctx => { return ctx.GetAudioPlayoutDevice(); });
            }).Result;
        }
        public void SetAudioPlayoutDevice(MediaDevice device)
        {
            Task.Run(async () => {
                await Context.WithContextAction<MediaDevice>((ctx, value) => { ctx.SetAudioPlayoutDevice(value); }, device);
            }).Wait();
        }

        public CaptureCapabilities GetVideoCaptureCapabilities(MediaDevice device)
        {
            return Task.Run<CaptureCapabilities>(() => {
                return Context.WithContextFunc<MediaDevice, CaptureCapabilities>((ctx, value) => { return ctx.GetVideoCaptureCapabilities(value); }, device);
            }).Result;
        }

        public IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device)
        {
            return Task.Run(async () =>
            {
                return await await Context.WithContextFunc<MediaDevice, Task< CaptureCapabilities>>(async (ctx, value) => { return await ctx.GetVideoCaptureCapabilitiesAsync(value); }, device);
            }).AsAsyncOperation();
        }

        public void SetPreferredVideoCaptureFormat(int width, int height, int frameRate)
        {
            Context.SetPreferredVideoCaptureFormat(new VideoCaptureFormat(width, height, frameRate));
        }
        public void SetPreferredVideoCaptureFormat(VideoCaptureFormat format)
        {
            Task.Run(async () => {
                await Context.WithContextAction<VideoCaptureFormat>((ctx, value) => { ctx.SetPreferredVideoCaptureFormat(value); }, format);
            }).Wait();
        }

        public void InitializeMedia()
        {
            // TODO please remove this method and auto-init WebRTC as needed (Fred LaBel)
            Task.Run(async () =>
            {
                await Context.WithContextAction(async ctx => { await ctx.InitializeMediaAsync(); });
            });
        }

        public IAsyncAction InitializeMediaAsync()
        {
            // TODO please remove this method and auto-init WebRTC as needed (Fred LaBel)
            return Task.Run(async () =>
            {
                await Context.WithContextAction(async ctx => { await ctx.InitializeMediaAsync(); });
            }).AsAsyncAction();
        }

        public void StartTrace()
        {
            Task.Run(async () => {
                await Context.WithContextAction(ctx => { ctx.StartTrace(); });
            }).Wait();
        }

        public void StopTrace()
        {
            Task.Run(async () => {
                await Context.WithContextAction(ctx => { ctx.StopTrace(); });
            }).Wait();
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
            Task.Run(async () => {
                await Context.WithContextAction<TraceServerConfig>((ctx, value) => { ctx.SaveTrace(value); }, traceServer);
            }).Wait();
        }

        public void ReleaseDevices()
        {
            Task.Run(async () => {
                await Context.WithContextAction(ctx => { ctx.ReleaseDevices(); });
            }).Wait();
        }

        #endregion
    }
}
