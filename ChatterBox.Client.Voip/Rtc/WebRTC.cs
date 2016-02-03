
#if USE_WEBRTC_API

using System;

namespace ChatterBox.Client.Voip.Rtc
{
    using DtoCodecInfo = ChatterBox.Client.Common.Media.Dto.CodecInfo;
    using RtcHelper = ChatterBox.Client.Voip.Rtc.Helper;
    using RTCSessionDescription = webrtc_winrt_api.RTCSessionDescription;
    using RTCPeerConnection = webrtc_winrt_api.RTCPeerConnection;
    using Media = webrtc_winrt_api.Media;
    using MediaDevice = webrtc_winrt_api.MediaDevice;
    using SdpUtils = ChatterBox.Client.Voip.Utils.SdpUtils;

    internal sealed class Helper
    {
        public static void SelectCodecs(
            RTCSessionDescription description,
            DtoCodecInfo audioCodec,
            DtoCodecInfo videoCodec
            )
        {
            var sdpString = description.Sdp;
            SdpUtils.SelectCodecs(ref sdpString, DtoExtensions.FromDto(audioCodec), DtoExtensions.FromDto(videoCodec));
            description.Sdp = sdpString;
        }

        public static bool SelectAudioPlayoutDevice(
            RTCPeerConnection peerConnection,
            Media media,
            MediaDevice device
            )
        {
            if (null == device) return false;
            if (null == media) return false;
            return media.SelectAudioPlayoutDevice(device);
        }
    }
}

#endif //USE_WERBRTC_API
