
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

    internal sealed class Helper
    {
        public static void SelectCodecs(RTCSessionDescription description, DtoCodecInfo codec)
        {
#warning TODO NEED IMPLEMENTATION
            throw new NotImplementedException();
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
