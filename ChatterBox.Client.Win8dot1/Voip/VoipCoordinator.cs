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
using ChatterBox.Client.Voip.States.Interfaces;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Threading.Tasks;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VoipCoordinator : IVoipCoordinator
    {
        // On win8.1 the functionality for voip call is not available
        // so this implementation is dummy.

        public void SetActiveCall(OutgoingCallRequest request)
        {
        }

        public void SetActiveIncomingCall(RelayMessage message, bool videoEnabled)
        {
        }

        public void StartIncomingCall(RelayMessage message)
        {
        }

        public void StartOutgoingCall(OutgoingCallRequest request)
        {
        }

        public Task StartVoipTask()
        {
            return Task.Run(() => { });
        }

        public void StopVoip()
        {
        }
    }
}
