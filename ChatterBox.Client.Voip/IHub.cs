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
using ChatterBox.Common.Communication.Messages.Relay;
using System.Collections.Generic;

#if USE_WEBRTC_API
using RtcPeerConnection = webrtc_winrt_api.RTCPeerConnection;
#elif USE_ORTC_API
using RtcPeerConnection = ChatterBox.Client.Voip.Rtc.RTCPeerConnection;
#endif //USE_WEBRTC_API

namespace ChatterBox.Client.Voip
{
    internal interface IHub
    {
        void Relay(RelayMessage message);

        void OnVoipState(VoipState voipState);

        void InitialiazeStatsManager(RtcPeerConnection pc);

        void ToggleStatsManagerConnectionState(bool enable);

        void OnUpdateFrameFormat(FrameFormat frameFormat);

        void OnUpdateFrameRate(FrameRate frameRate);

        void TrackStatsManagerEvent(string name, IDictionary<string, string> props);

        void TrackStatsManagerMetric(string name, double value);

        void StartStatsManagerCallWatch();

        void StopStatsManagerCallWatch();

        bool IsAppInsightsEnabled
        {
            get;
            set;
        }
    }
}
