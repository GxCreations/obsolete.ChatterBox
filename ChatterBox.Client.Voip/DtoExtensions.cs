using System.Linq;
using ChatterBox.Client.Common.Signaling.Dto;

#if USE_WEBRTC_API
using RtcMediaStream = webrtc_winrt_api.MediaStream;
using RtcEngine = webrtc_winrt_api.WebRTC;
using RtcMedia = webrtc_winrt_api.Media;
using RtcPeerConnection = webrtc_winrt_api.RTCPeerConnection;
using RtcIceCandidate = webrtc_winrt_api.RTCIceCandidate;
using RtcMediaDevice = webrtc_winrt_api.MediaDevice;
using RtcCodecInfo = webrtc_winrt_api.CodecInfo;
using RtcCaptureCapability = webrtc_winrt_api.CaptureCapability;
#elif USE_ORTC_API
#endif //USE_WEBRTC_API

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
        public static RtcIceCandidate FromDto(this DtoIceCandidate obj)
        {
            return new RtcIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static DtoIceCandidate ToDto(this RtcIceCandidate obj)
        {
            return new DtoIceCandidate
            {
                Candidate = obj.Candidate,
                SdpMid = obj.SdpMid,
                SdpMLineIndex = obj.SdpMLineIndex
            };
        }

        public static RtcIceCandidate[] FromDto(this DtoIceCandidates obj)
        {
            return obj.Candidates.Select(FromDto).ToArray();
        }

        public static DtoIceCandidates ToDto(this RtcIceCandidate[] obj)
        {
            return new DtoIceCandidates
            {
                Candidates = obj.Select(ToDto).ToArray()
            };
        }

        public static RtcMediaDevice FromDto(this DtoMediaDevice obj)
        {
            return new RtcMediaDevice(obj.Id, obj.Name);
        }

        public static DtoMediaDevice ToDto(this RtcMediaDevice obj)
        {
            return new DtoMediaDevice
            {
                Id = obj.Id,
                Name = obj.Name,
            };
        }

        public static RtcMediaDevice[] FromDto(this DtoMediaDevices obj)
        {
            return obj.Devices.Select(FromDto).ToArray();
        }

        public static DtoMediaDevices ToDto(this RtcMediaDevice[] obj)
        {
            return new DtoMediaDevices
            {
                Devices = obj.Select(ToDto).ToArray()
            };
        }

        public static RtcCodecInfo FromDto(this DtoCodecInfo obj)
        {
            return new RtcCodecInfo(obj.Id, obj.Clockrate, obj.Name);
        }

        public static DtoCodecInfo ToDto(this RtcCodecInfo obj)
        {
            return new DtoCodecInfo
            {
                Id = obj.Id,
                Name = obj.Name,
                Clockrate = obj.Clockrate,
            };
        }

        public static RtcCodecInfo[] FromDto(this DtoCodecInfos obj)
        {
            return obj.Codecs.Select(FromDto).ToArray();
        }

        public static DtoCodecInfos ToDto(this RtcCodecInfo[] obj)
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

        public static DtoCaptureCapability ToDto(this RtcCaptureCapability obj)
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

        public static DtoCaptureCapabilities ToDto(this RtcCaptureCapability[] obj)
        {
            return new DtoCaptureCapabilities
            {
                Capabilities = obj.Select(ToDto).ToArray()
            };
        }

    }
}
