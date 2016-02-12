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

using ChatterBox.Common.Communication.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Standard;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Common.Communication.Messages.Relay;

namespace ChatterBox.Client.Win8dot1.Channels
{
    internal class ClientChannel : IClientChannel
    {
        private SignalingClient _signalingClient;

        public ClientChannel(SignalingClient signalingClient)
        {
            _signalingClient = signalingClient;
        }

        public void ClientConfirmation(Confirmation confirmation)
        {
            _signalingClient.ClientConfirmation(confirmation);
        }

        public void ClientHeartBeat()
        {
            _signalingClient.ClientHeartBeat();
        }

        public void GetPeerList(Message message)
        {
            _signalingClient.GetPeerList(message);
        }

        public void Register(Registration message)
        {
            _signalingClient.Register(message);
        }

        public void Relay(RelayMessage message)
        {
            _signalingClient.Relay(message);
        }
    }
}
