//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System.Linq;
using ChatterBox.Client.Common.Signaling.Dto;
using webrtc_winrt_api;
using DtoMediaDevice = ChatterBox.Client.Common.Media.Dto.MediaDevice;
using DtoMediaDevices = ChatterBox.Client.Common.Media.Dto.MediaDevices;
using DtoCodecInfo = ChatterBox.Client.Common.Media.Dto.CodecInfo;
using DtoCodecInfos = ChatterBox.Client.Common.Media.Dto.CodecInfos;
using DtoMediaRatio = ChatterBox.Client.Common.Media.Dto.MediaRatio;
using DtoCaptureCapability = ChatterBox.Client.Common.Media.Dto.CaptureCapability;
using DtoCaptureCapabilities = ChatterBox.Client.Common.Media.Dto.CaptureCapabilities;
using MediaRatio = Windows.Media.MediaProperties.MediaRatio;
using System.Collections.Generic;

namespace ChatterBox.Client.Voip
{
    internal static class DtoExtensions
    {
        public static RTCIceCandidate FromDto(this DtoIceCandidate obj)
        {
            return new RTCIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static DtoIceCandidate ToDto(this RTCIceCandidate obj)
        {
            return new DtoIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static RTCIceCandidate[] FromDto(this DtoIceCandidates obj)
        {
            return obj.Candidates.Select(FromDto).ToArray();
        }

        public static DtoIceCandidates ToDto(this RTCIceCandidate[] obj)
        {
            return new DtoIceCandidates
            {
                Candidates = obj.Select(ToDto).ToArray()
            };
        }

        public static MediaDevice FromDto(this DtoMediaDevice obj)
        {
            return new MediaDevice(obj.Id, obj.Name);
        }

        public static DtoMediaDevice ToDto(this MediaDevice obj)
        {
            return new DtoMediaDevice
            {
                Id = obj.Id,
                Name = obj.Name,
            };
        }

        public static MediaDevice[] FromDto(this DtoMediaDevices obj)
        {
            return obj.Devices.Select(FromDto).ToArray();
        }

        public static DtoMediaDevices ToDto(this MediaDevice[] obj)
        {
            return new DtoMediaDevices
            {
                Devices = obj.Select(ToDto).ToArray()
            };
        }

        public static CodecInfo FromDto(this DtoCodecInfo obj)
        {
            return new CodecInfo(obj.Id, obj.Clockrate, obj.Name);
        }

        public static DtoCodecInfo ToDto(this CodecInfo obj)
        {
            return new DtoCodecInfo
            {
                Id = obj.Id,
                Name = obj.Name,
                Clockrate = obj.Clockrate,
            };
        }

        public static CodecInfo[] FromDto(this DtoCodecInfos obj)
        {
            return obj.Codecs.Select(FromDto).ToArray();
        }

        public static DtoCodecInfos ToDto(this CodecInfo[] obj)
        {
            return new DtoCodecInfos
            {
                Codecs = obj.Select(ToDto).ToArray()
            };
        }

        public static MediaRatio FromDto(this DtoMediaRatio obj)
        {
            MediaRatio ratio = null;

            ratio.Numerator = obj.Numerator;
            ratio.Denominator = obj.Denominator;

            return ratio;
        }

        public static DtoMediaRatio ToDto(this MediaRatio obj)
        {
            return new DtoMediaRatio
            {
                Numerator = obj.Numerator,
                Denominator = obj.Denominator
            };
        }

        public static DtoCaptureCapability ToDto(this CaptureCapability obj)
        {
            return new DtoCaptureCapability
            {
                FrameRate = obj.FrameRate,
                FrameRateDescription = obj.FrameRateDescription,
                FullDescription = obj.FullDescription,
                Height = obj.Height,
                PixelAspectRatio = ToDto(obj.PixelAspectRatio),
                ResolutionDescription = obj.ResolutionDescription,
                Width = obj.Width
            };
        }

        public static DtoCaptureCapabilities ToDto(this CaptureCapability[] obj)
        {
            return new DtoCaptureCapabilities
            {
                Capabilities = obj.Select(ToDto).ToArray()
            };
        }

    }
}
