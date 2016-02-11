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

using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Foreground.Dto.ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Common.Communication.Foreground
{
    public interface IForegroundChannel
    {
        void OnSignaledPeerDataUpdated();
        void OnSignaledRegistrationStatusUpdated();
        void OnSignaledRelayMessagesUpdated();
        void OnVoipState(VoipState state);
        void OnUpdateFrameFormat(FrameFormat frameFormat);
        void OnUpdateFrameRate(FrameRate frameRate);
        ForegroundState GetForegroundState();
        string GetShownUserId();
    }
}