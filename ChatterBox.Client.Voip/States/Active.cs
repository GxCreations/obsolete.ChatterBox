﻿using System;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Client.Voip.States.Interfaces;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using System.Linq;
using System.Collections.Generic;

#if USE_WEBRTC_API
using RtcMediaStream = webrtc_winrt_api.MediaStream;
using RtcIceCandidate = webrtc_winrt_api.RTCIceCandidate;
#elif USE_ORTC_API
using RtcMediaStream = ChatterBox.Client.Voip.Rtc.MediaStream;
using RtcIceCandidate = ortc_winrt_api.RTCIceCandidate;
#endif //USE_WEBRTC_API

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_ActiveCall : BaseVoipState
    {
        private OutgoingCallRequest _callRequest;

        public VoipState_ActiveCall(OutgoingCallRequest callRequest)
        {
            _callRequest = callRequest;
        }
        public override async Task OnEnteringState()
        {
            Context.TrackCallStarted();
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
        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.ActiveCall;
            }
        }

        public override async Task Hangup()
        {
            Context.TrackCallEnded();
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnIceCandidate(RelayMessage message)
        {
            var candidates = (DtoIceCandidates)JsonConvert.Deserialize(message.Payload, typeof(DtoIceCandidates));
            foreach (var candidate in candidates.Candidates)
            {
                await Context.PeerConnection.AddIceCandidate(candidate.FromDto());
            }
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            Context.TrackCallEnded();
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task SendLocalIceCandidates(RtcIceCandidate[] candidates)
        {
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(candidates.ToDto()));
        }
    }
}