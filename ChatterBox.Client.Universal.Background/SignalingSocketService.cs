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
using System.Diagnostics;
using Windows.Networking;
using Windows.Networking.Sockets;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Signaling.PersistedData;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class SignalingSocketService : ISignalingSocketService, ISignalingSocketChannel
    {
        public ISignalingSocketOperation SocketOperation => new SignalingSocketOperation();

        #region ISignalingSocketChannel Members
        public void DisconnectSignalingServer()
        {
            SocketOperation.Disconnect();
            SignaledPeerData.Reset();
            SignalingStatus.Reset();
            SignaledRelayMessages.Reset();
        }

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            try
            {
                SignaledPeerData.Reset();
                SignalingStatus.Reset();
                SignaledRelayMessages.Reset();

                var socket = new StreamSocket();
                socket.EnableTransferOwnership(Guid.Parse(connectionOwner.OwnerId),
                    SocketActivityConnectedStandbyAction.Wake);
                socket.ConnectAsync(new HostName(SignalingSettings.SignalingServerHost),
                    SignalingSettings.SignalingServerPort, SocketProtectionLevel.PlainSocket)
                    .AsTask()
                    .Wait();
                socket.TransferOwnership(SignalingSocketOperation.SignalingSocketId);
                return new ConnectionStatus
                {
                    IsConnected = true
                };
            }
            catch (Exception exception)
            {
                Debug.Write("Failed to connect to signalling server: ex: " + exception.Message);
                return new ConnectionStatus
                {
                    IsConnected = false
                };
            }
        }

        public ConnectionStatus GetConnectionStatus()
        {
            using (var socketOperation = SocketOperation)
            {
                return new ConnectionStatus
                {
                    IsConnected = socketOperation.Socket != null
                };
            }
        }

        #endregion
    }
}