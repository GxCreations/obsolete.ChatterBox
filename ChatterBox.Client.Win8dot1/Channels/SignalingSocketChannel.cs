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

using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Signaling.PersistedData;
using Microsoft.Practices.Unity;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Win8dot1.Channels
{
    internal class SignalingSocketChannel : ISignalingSocketChannel
    {
        private StreamSocket _streamSocket;
        private bool _isConnected;
        private SignalingClient _signalingClient;
        private IUnityContainer _unityContainer;
        public void DisconnectSignalingServer()
        {
            SignaledPeerData.Reset();
            SignalingStatus.Reset();
            SignaledRelayMessages.Reset();
            if (_streamSocket != null)
                _streamSocket.Dispose();
            _streamSocket = null;
            _isConnected = false;

        }

        public SignalingSocketChannel(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner)
        {
            SignaledPeerData.Reset();
            SignalingStatus.Reset();
            SignaledRelayMessages.Reset();
            _streamSocket = new StreamSocket();
            _streamSocket.ConnectAsync(new HostName(SignalingSettings.SignalingServerHost),
                                       SignalingSettings.SignalingServerPort, SocketProtectionLevel.PlainSocket)
                         .AsTask()
                         .Wait();

            _isConnected = true;

            _signalingClient = _unityContainer.Resolve<SignalingClient>();

            StartReading();

            return new ConnectionStatus
            {
                IsConnected = _isConnected
            };
        }

        private void StartReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    var reader = new StreamReader(_streamSocket.InputStream.AsStreamForRead());
                    while (_isConnected)
                    {
                        var message = await reader.ReadLineAsync();
                        if (message == null) break;
                        _signalingClient.HandleRequest(message);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Signaling socket read error: {exception.Message}");
                    _signalingClient.ServerConnectionError();
                    _isConnected = false;
                }
            });
        }

        public ConnectionStatus GetConnectionStatus()
        {
            return new ConnectionStatus
            {
                IsConnected = _isConnected
            };
        }

        public StreamSocket StreamSocket { get { return _streamSocket; } }

        public void Dispose()
        {

        }
    }
}
