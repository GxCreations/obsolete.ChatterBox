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

using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Common.Communication.Serialization;
using System.Threading;

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_LocalRinging : BaseVoipState
    {
        private readonly RelayMessage _message;
        private readonly OutgoingCallRequest _callRequest;
        private Timer _callTimeout;
        private const int _callDueTimeout = 1000 * 35; //35 seconds, should be bigger than RemoteRinging state timer

        public VoipState_LocalRinging(RelayMessage message)
        {
            _message = message;
            _callRequest = (OutgoingCallRequest)JsonConvert.Deserialize(message.Payload, typeof(OutgoingCallRequest));
        }

        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.LocalRinging;
            }
        }

        public override async Task Answer()
        {
            Context.SendToPeer(RelayMessageTags.VoipAnswer, "");

            var establishIncomingState = new VoipState_EstablishIncoming(_message);
            await Context.SwitchState(establishIncomingState);
        }

        public override async Task Hangup()
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task OnEnteringState()
        {
            Debug.Assert(Context.PeerConnection == null);
            Context.PeerId = _message.FromUserId;
            Context.IsVideoEnabled = _callRequest.VideoEnabled;

            Context.VoipCoordinator.StartIncomingCall(_message);

            _callTimeout = new Timer(CallTimeoutCallback, null, _callDueTimeout, Timeout.Infinite);
        }

        public override async Task OnRemoteHangup(RelayMessage message)
        {
            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        public override async Task Reject(IncomingCallReject reason)
        {
            Context.SendToPeer(RelayMessageTags.VoipReject, "Rejected");

            var hangingUpState = new VoipState_HangingUp();
            await Context.SwitchState(hangingUpState);
        }

        private async void CallTimeoutCallback(object state)
        {
            if (Context != null)
            {
                await Hangup();
            }
            else
            {
                StopTimer();
            }
        }

        public override Task OnLeavingState()
        {
            StopTimer();
            return base.OnLeavingState();
        }

        private void StopTimer()
        {
            if (_callTimeout != null)
            {
                _callTimeout.Dispose();
                _callTimeout = null;
            }
        }
    }
}
