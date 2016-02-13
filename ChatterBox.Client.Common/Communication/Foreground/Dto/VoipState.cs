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

namespace ChatterBox.Client.Common.Communication.Foreground.Dto
{
    public enum VoipStateEnum
    {
        Idle,
        LocalRinging,
        RemoteRinging,
        EstablishOutgoing,
        EstablishIncoming,
        HangingUp,
        ActiveCall
    }

    public sealed class VoipState
    {
        public bool HasPeerConnection { get; set; }
        public bool IsVideoEnabled { get; set; }
        public string PeerId { get; set; }
        public VoipStateEnum State { get; set; }
    }
}