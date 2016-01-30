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
                await Context.WithContextAction(ctx => { ctx.SetVideoDevice(device); });
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
                await Context.WithContextAction(ctx => { ctx.SetAudioDevice(device); });
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
                await Context.WithContextAction(ctx => { ctx.SetVideoCodec(codec); });
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
                await Context.WithContextAction(ctx => { ctx.SetAudioCodec(codec); });
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
                await Context.WithContextAction(ctx => { ctx.SetAudioPlayoutDevice(device); });
            }).Wait();
        }

        public CaptureCapabilities GetVideoCaptureCapabilities(MediaDevice device)
        {
            return Task.Run<CaptureCapabilities>(() => {
                return Context.WithContextFunc(ctx => { return ctx.GetVideoCaptureCapabilities(device); });
            }).Result;
        }

        public IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device)
        {
            return Context.WithContextFuncAsync(ctx => { return ctx.GetVideoCaptureCapabilitiesAsync(device); }).AsAsyncOperation();
        }

        public void SetPreferredVideoCaptureFormat(VideoCaptureFormat format)
        {
            Task.Run(async () => {
                await Context.WithContextAction(ctx => { ctx.SetPreferredVideoCaptureFormat(format); });
            }).Wait();
        }

        public void InitializeMedia()
        {
            // TODO please remove this method and auto-init WebRTC as needed (Fred LaBel)
            Context.WithContextActionAsync(async ctx => { await ctx.InitializeMediaAsync(); }).Wait();
        }

        public IAsyncAction InitializeMediaAsync()
        {
            // TODO please remove this method and auto-init WebRTC as needed (Fred LaBel)
            return Task.Run(async () =>
            {
                await Context.WithContextActionAsync(async ctx => { await ctx.InitializeMediaAsync(); });
            }).AsAsyncAction();
        }

        public IAsyncOperation<bool> RequestAccessForMediaCaptureAsync()
        {
            // do not call for Windows 10
            throw new System.NotSupportedException();
        }

        public void SyncWithNTP(long ntpTime)
        {
            Context.WithContextAction(ctx => { ctx.SyncWithNTP(ntpTime); }).Wait();
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

        public void SaveTrace(TraceServerConfig traceServer)
        {
            Task.Run(async () => {
                await Context.WithContextAction(ctx => { ctx.SaveTrace(traceServer); });
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
