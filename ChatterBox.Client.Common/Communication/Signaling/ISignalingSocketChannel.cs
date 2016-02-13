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

using ChatterBox.Client.Common.Communication.Signaling.Dto;

namespace ChatterBox.Client.Common.Communication.Signaling
{
    public interface ISignalingSocketChannel
    {
        ConnectionStatus ConnectToSignalingServer(ConnectionOwner connectionOwner);
        void DisconnectSignalingServer();
        ConnectionStatus GetConnectionStatus();
    }
}