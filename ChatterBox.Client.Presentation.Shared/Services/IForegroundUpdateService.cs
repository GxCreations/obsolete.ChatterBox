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
using ChatterBox.Client.Common.Communication.Foreground.Dto;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface IForegroundUpdateService
    {
        event Action OnPeerDataUpdated;
        event Action OnRegistrationStatusUpdated;
        event Action OnRelayMessagesUpdated;
        event Action<VoipState> OnVoipStateUpdate;
        event Action<FrameFormat> OnFrameFormatUpdate;
        event Action<FrameRate> OnFrameRateUpdate;
        event Func<string> GetShownUser;
    }
}