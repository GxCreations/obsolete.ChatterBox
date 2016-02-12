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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Signaling.Dto;
using ChatterBox.Client.Voip;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Serialization;
using webrtc_winrt_api;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Core;
using ChatterBox.Client.Voip.Utils;
using ChatterBox.Client.Common.Settings;

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_EstablishOutgoing : BaseVoipState
    {
        private OutgoingCallRequest _callRequest;

        public VoipState_EstablishOutgoing(OutgoingCallRequest request)
        {
            _callRequest = request;
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.EstablishOutgoing;
            }
        }

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);

            Context.VoipCoordinator.StartOutgoingCall(_callRequest);

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
            var sdpOffer = await Context.PeerConnection.CreateOffer();
            var sdpString = sdpOffer.Sdp;
            SdpUtils.SelectCodecs(ref sdpString, DtoExtensions.FromDto(Context.GetAudioCodec()), DtoExtensions.FromDto(Context.GetVideoCodec()));
            sdpOffer.Sdp = sdpString;
            await Context.PeerConnection.SetLocalDescription(sdpOffer);

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

            Context.SendToPeer(RelayMessageTags.SdpOffer, sdpOffer.Sdp);
        }

        internal override async Task OnAddStream(MediaStream stream)
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

        public override async Task OnSdpAnswer(RelayMessage message)
        {
            await
                Context.PeerConnection.SetRemoteDescription(new RTCSessionDescription(RTCSdpType.Answer, message.Payload));
            await Context.SwitchState(new VoipState_ActiveCall(_callRequest));
        }

        public override async Task SendLocalIceCandidates(RTCIceCandidate[] candidates)
        {
            Context.SendToPeer(RelayMessageTags.IceCandidate, JsonConvert.Serialize(candidates.ToDto()));
        }
    }
}
