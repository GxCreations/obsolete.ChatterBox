﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

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
            return _voipContext.GetVideoCaptureCapabilities(device);
        }
        public IAsyncOperation<CaptureCapabilities> GetVideoCaptureCapabilitiesAsync(MediaDevice device)
        {
            return _voipContext.GetVideoCaptureCapabilitiesAsync(device).AsAsyncOperation();
        }

        public void SetPreferredVideoCaptureFormat(VideoCaptureFormat format)
        {
            _voipContext.SetPreferredVideoCaptureFormat(format);
        }

        public IAsyncOperation<bool> RequestAccessForMediaCaptureAsync()
        {
            return _voipContext.RequestAccessForMediaCaptureAsync().AsAsyncOperation();
        }

        public void SyncWithNTP(long ntpTime)
        {
            _voipContext.SyncWithNTP(ntpTime);
        }

        public void StartTrace()
        {
            _voipContext.StartTrace();
        }

        public void StopTrace()
        {
            _voipContext.StopTrace();
        }
        public void SaveTrace(TraceServerConfig traceServer)
        {
            _voipContext.SaveTrace(traceServer);
        }

        public void ToggleETWStats(bool enabled)
        {
            _voipContext.ToggleETWStats(enabled);
        }

        public void ReleaseDevices()
        {
            _voipContext.ReleaseDevices();
        }
    }
}
