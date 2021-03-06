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

using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public interface IVoipChannel
    {
        void InitializeRTC();
        void SetForegroundProcessId(uint processId);
        void DisplayOrientationChanged(DisplayOrientations orientation);
        void Answer();
        // Locally initiated calls
        void Call(OutgoingCallRequest request);
        VoipState GetVoipState();
        // Hangup can happen on both sides
        void Hangup();
        void OnIceCandidate(RelayMessage message);
        // Remotely initiated calls
        void OnIncomingCall(RelayMessage message);
        void OnOutgoingCallAccepted(RelayMessage message);
        void OnOutgoingCallRejected(RelayMessage message);
        void OnRemoteHangup(RelayMessage message);
        // WebRTC signaling
        void OnSdpAnswer(RelayMessage message);
        void OnSdpOffer(RelayMessage message);
        void Reject(IncomingCallReject reason);
        void RegisterVideoElements(MediaElement self, MediaElement peer);

        void ConfigureMicrophone(MicrophoneConfig config);

        void SuspendVoipVideo();
        void ResumeVoipVideo();
        void ConfigureVideo(VideoConfig config);
        void OnRemoteControlSize(VideoControlSize size);
        void OnLocalControlSize(VideoControlSize size);
    }
}
