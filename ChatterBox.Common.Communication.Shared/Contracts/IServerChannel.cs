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

using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;

namespace ChatterBox.Common.Communication.Contracts
{
    public interface IServerChannel
    {
        void OnPeerList(PeerList peerList);
        void OnPeerPresence(PeerUpdate peer);
        void OnRegistrationConfirmation(RegisteredReply reply);
        void ServerConfirmation(Confirmation confirmation);
        void ServerError(ErrorReply reply);
        void ServerHeartBeat();
        void ServerReceivedInvalidMessage(InvalidMessage reply);
        void ServerRelay(RelayMessage message);
        void ServerConnectionError();
    }
}