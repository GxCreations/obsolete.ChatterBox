﻿using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Voip.States.Interfaces;
using System;
using Microsoft.Practices.Unity;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using System.Threading.Tasks;

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal class VoipState_Idle : BaseVoipState
    {
        public override VoipStateEnum VoipState
        {
            get
            {
                return VoipStateEnum.Idle;
            }
        }

        public override async Task Call(OutgoingCallRequest request)
        {
            var remoteRingingState = new VoipState_RemoteRinging(request);
            await Context.SwitchState(remoteRingingState);
        }

        public override async Task OnEnteringState()
        {
            // Entering idle state.
            Context.VoipCoordinator.StopVoip();

            // Make sure the context is sane.
            Context.PeerConnection = null;
            Context.PeerId = null;
        }

        public override async Task OnIncomingCall(RelayMessage message)
        {
            var localRingingState = new VoipState_LocalRinging(message);
            await Context.SwitchState(localRingingState);
        }

        public override async Task OnLeavingState()
        {
            await Context.VoipCoordinator.StartVoipTask();
            await Context.InitializeWebRTC();
        }
    }
}
