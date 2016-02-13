﻿//*********************************************************
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
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Common.Communication.Messages.Standard;
using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ChatterBox.Client.Console
{
    public class ChatterBoxConsoleClient : IClientChannel, IServerChannel
    {
        private StreamReader _reader;
        private string _userId;
        private StreamWriter _writer;

        public ChatterBoxConsoleClient()
        {
            ServerReadProxy = new ChannelInvoker(this);
        }

        private ChannelWriteHelper ChannelWriteHelper { get; } = new ChannelWriteHelper(typeof(IClientChannel));
        public string Label { get; set; }

        private ILog Logger =>
            LogManager.GetLogger(string.IsNullOrWhiteSpace(Label) ? nameof(ChatterBoxConsoleClient) : Label);

        private ChannelInvoker ServerReadProxy { get; }
        private TcpClient TcpClient { get; } = new TcpClient();
        private ConcurrentQueue<string> WriteQueue { get; } = new ConcurrentQueue<string>();

        #region IClientChannel Members

        public void ClientConfirmation(Confirmation confirmation)
        {
            SendToServer(confirmation);
        }

        public void ClientHeartBeat()
        {
            SendToServer();
        }

        public void GetPeerList(Message message)
        {
            SendToServer(message);
        }

        public void Register(Registration request)
        {
            _userId = request.UserId;
            SendToServer(request);
        }

        public void Relay(RelayMessage message)
        {
            SendToServer(message);
        }

        #endregion

        #region IServerChannel Members

        public void OnPeerList(PeerList peerList)
        {
            ClientConfirmation(Confirmation.For(peerList));
            foreach (var peer in peerList.Peers)
            {
                Logger.Trace($"{peer.Name} - {(peer.IsOnline ? "Online" : "Offline")}");
            }
        }

        public void OnPeerPresence(PeerUpdate peer)
        {
            ClientConfirmation(Confirmation.For(peer));
            GetPeerList(new Message());
        }

        public void OnRegistrationConfirmation(RegisteredReply reply)
        {
            ClientConfirmation(Confirmation.For(reply));
        }

        public void ServerConfirmation(Confirmation confirmation)
        {
        }

        public void ServerError(ErrorReply reply)
        {
            Logger.Error($"Server signaled error for message {reply.ReplyFor} with message {reply.ErrorMessage}");
        }

        public void ServerHeartBeat()
        {
            ClientHeartBeat();
        }

        public void ServerReceivedInvalidMessage(InvalidMessage reply)
        {
            Logger.Error($"Server signaled invalid message: {reply.OriginalMessage}");
        }

        public void ServerRelay(RelayMessage message)
        {
            ClientConfirmation(Confirmation.For(message));
            if (message.Tag == RelayMessageTags.InstantMessage)
            {
                Relay(new RelayMessage
                {
                    SentDateTimeUtc = DateTimeOffset.UtcNow,
                    ToUserId = message.FromUserId,
                    FromUserId = _userId,
                    Tag = message.Tag,
                    Payload = $"Received: {message.Payload}"
                });
            }
        }

        public void ServerConnectionError()
        {

        }

        #endregion

        public void Connect(string host = "localhost", int port = 50000)
        {
            TcpClient.Connect(host, port);
            var networkStream = TcpClient.GetStream();
            _reader = new StreamReader(networkStream);
            _writer = new StreamWriter(networkStream) { AutoFlush = true };
            StartReading();
            StartWriting();
        }

        private void SendToServer(object arg = null, [CallerMemberName] string method = null)
        {
            WriteQueue.Enqueue(ChannelWriteHelper.FormatOutput(arg, method));
        }

        private void StartReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        var request = await _reader.ReadLineAsync();
                        if (request == null) break;
                        Logger.Debug($"RECEIVED: {request}");
                        ServerReadProxy.ProcessRequest(request);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn("Disconnected during socket read operation due to exception", exception);
                }
            });
        }

        private void StartWriting()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        while (!WriteQueue.IsEmpty)
                        {
                            string message;
                            if (!WriteQueue.TryDequeue(out message)) continue;
                            Logger.Trace($"SENT: {message}");
                            await _writer.WriteLineAsync(message);
                        }
                        await Task.Delay(200);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn("Disconnected during socket WRITE operation due to exception", exception);
                }
            });
        }
    }
}