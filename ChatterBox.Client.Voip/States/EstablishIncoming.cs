﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using System.Linq;
using Windows.UI.Core;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.Utils;
using ChatterBox.Client.Common.Settings;

#if USE_WEBRTC_API
using RtcConfiguration = webrtc_winrt_api.RTCConfiguration;
using RtcPeerConnection = webrtc_winrt_api.RTCPeerConnection;
using RtcMediaStream = webrtc_winrt_api.MediaStream;
using RtcSessionDescription = webrtc_winrt_api.RTCSessionDescription;
using RtcMediaStreamConstraints = webrtc_winrt_api.RTCMediaStreamConstraints;
using RtcNegotiationType = webrtc_winrt_api.RTCSdpType;
using RtcIceCandidate = webrtc_winrt_api.RTCIceCandidate;
#elif USE_ORTC_API
using RtcConfiguration = ChatterBox.Client.Voip.Rtc.RTCConfiguration;
using RtcPeerConnection = ChatterBox.Client.Voip.Rtc.RTCPeerConnection;
using RtcMediaStream = ChatterBox.Client.Voip.Rtc.MediaStream;
using RtcSessionDescription = ChatterBox.Client.Voip.Rtc.RTCSessionDescription;
using RtcMediaStreamConstraints = ChatterBox.Client.Voip.Rtc.RTCMediaStreamConstraints;
using RtcNegotiationType = ChatterBox.Client.Voip.Rtc.RTCSdpType;
using RtcIceCandidate = ortc_winrt_api.RTCIceCandidate;
#endif //USE_WEBRTC_API

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishIncoming : BaseVoipState
    {
        private RelayMessage _message;
        private OutgoingCallRequest _callRequest;

        public VoipState_EstablishIncoming(RelayMessage message)
        {
            _message = message;
            _callRequest = (OutgoingCallRequest)JsonConvert.Deserialize(message.Payload, typeof(OutgoingCallRequest));
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.EstablishIncoming;
            }
        }

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.VoipCoordinator.SetActiveIncomingCall(_message, _callRequest.VideoEnabled);

            var config = new RtcConfiguration
            {
                IceServers = WebRtcSettingsUtils.ToRTCIceServer(IceServerSettings.IceServers)
            };
            Context.PeerConnection = new RtcPeerConnection(config);

            Context.LocalStream = await Context.Media.GetUserMedia(new RtcMediaStreamConstraints
            {
                videoEnabled = _callRequest.VideoEnabled,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(Context.LocalStream);

            var tracks = Context.LocalStream.GetVideoTracks();
            if (tracks.Count > 0)
            {
#if WIN10
                var source = Context.Media.CreateMediaSource(tracks[0], VoipContext.LocalMediaStreamId);
#else
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, VoipContext.LocalMediaStreamId);
#endif
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source, Context.LocalVideoControlSize);
            }
        }

        internal override async Task OnAddStream(RtcMediaStream stream)
        {
            Context.RemoteStream = stream;
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
#if WIN10
                var source = Context.Media.CreateMediaSource(tracks[0], VoipContext.PeerMediaStreamId);
#else
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, VoipContext.PeerMediaStreamId);
#endif
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source, Context.RemoteVideoControlSize);
            }
        }

        public override async Task OnIceCandidate(RelayMessage message)
        {
            var candidates = (DtoIceCandidates)JsonConvert.Deserialize(message.Payload, typeof(DtoIceCandidates));
            foreach (var candidate in candidates.Candidates)
            {
                await Context.PeerConnection.AddIceCandidate(candidate.FromDto());
            }
        }

        public override async Task OnOffer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RtcSessionDescription(RtcNegotiationType.Offer, message.Payload));
            var sdpAnswer = await Context.PeerConnection.CreateAnswer();
            await Context.PeerConnection.SetLocalDescription(sdpAnswer);
            Context.SendToPeer(RelayMessageTags.SdpAnswer, sdpAnswer.Sdp);
            await Context.SwitchState(new VoipState_ActiveCall(_callRequest));
        }

        public override async Task SendLocalIceCandidates(RtcIceCandidate[] candidates)
        {
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(candidates.ToDto()));
        }
    }
}
