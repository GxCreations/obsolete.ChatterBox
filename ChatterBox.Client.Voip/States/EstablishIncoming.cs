﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using System.Linq;
using Windows.UI.Core;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Voip.Utils;
using ChatterBox.Client.Common.Settings;

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

            var config = new RTCConfiguration
            {
                IceServers = WebRtcSettingsUtils.ToRTCIceServer(IceServerSettings.IceServers)
            };
            Context.PeerConnection = new RTCPeerConnection(config);

            Context.LocalStream = await Context.Media.GetUserMedia(new RTCMediaStreamConstraints
            {
                videoEnabled = _callRequest.VideoEnabled,
                audioEnabled = true
            });
            Context.PeerConnection.AddStream(Context.LocalStream);

            var tracks = Context.LocalStream.GetVideoTracks();
            if (tracks.Count > 0)
            {
#if WIN10
                var source = Context.Media.CreateMediaSource(tracks[0], "LOCAL");
#else
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "LOCAL");
#endif
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
            }
        }

        internal override async Task OnAddStream(MediaStream stream)
        {
            Context.RemoteStream = stream;
            var tracks = stream.GetVideoTracks();
            if (tracks.Count > 0)
            {
#if WIN10
                var source = Context.Media.CreateMediaSource(tracks[0], "PEER");
#else
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, "PEER");
#endif
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source);
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
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Offer, message.Payload));
            var sdpAnswer = await Context.PeerConnection.CreateAnswer();
            await Context.PeerConnection.SetLocalDescription(sdpAnswer);
            Context.SendToPeer(RelayMessageTags.SdpAnswer, sdpAnswer.Sdp);
            await Context.SwitchState(new VoipState_ActiveCall(_callRequest));
        }

        public override async Task SendLocalIceCandidates(RTCIceCandidate[] candidates)
        {
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(candidates.ToDto()));
        }
    }
}
