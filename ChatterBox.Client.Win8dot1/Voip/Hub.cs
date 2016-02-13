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

using ChatterBox.Client.Voip;
using System;
using System.Diagnostics;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

#if USE_WEBRTC_API
using RtcPeerConnection = webrtc_winrt_api.RTCPeerConnection;
#elif USE_ORTC_API
using RtcPeerConnection = ChatterBox.Client.Voip.Rtc.RTCPeerConnection;
#endif //USE_WEBRTC_API


namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class Hub : IHub
    {
        private IUnityContainer _container;
        private SignalingClient _signalingClient;
        private IForegroundChannel _foregroundChannel;

        public Hub(IUnityContainer container)
        {
            _container = container;
        }

        public void OnVoipState(VoipState voipState)
        {
            if (_foregroundChannel == null)
            {
                _foregroundChannel = _container.Resolve<IForegroundChannel>();
            }
            _foregroundChannel.OnVoipState(voipState);
        }

        public void Relay(RelayMessage message)
        {
            if (_signalingClient == null)
            {
                _signalingClient = _container.Resolve<SignalingClient>();
            }
            _signalingClient.Relay(message);
        }

		public void InitialiazeStatsManager(RtcPeerConnection pc)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 client");
        }

        public void ToggleStatsManagerConnectionState(bool enable)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 client");
        }

        public void TrackStatsManagerEvent(string name, IDictionary<string, string> props)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 client");
        }

        public void TrackStatsManagerMetric(string name, double value)
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 client");
        }

        public void StartStatsManagerCallWatch()
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 client");
        }

        public void StopStatsManagerCallWatch()
        {
            Debug.WriteLine("Stats Manager is not integrated to 8.1 client");
        }

        public bool IsAppInsightsEnabled
        {
            get
            {
                return false;
            }
            set {/*AppInsights is not supported for 8.1*/}
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
            if (_foregroundChannel == null)
            {
                _foregroundChannel = _container.Resolve<IForegroundChannel>();
            }
            _foregroundChannel.OnUpdateFrameFormat(frameFormat);
        }

        public void OnUpdateFrameRate(FrameRate frameRate)
        {
            if (_foregroundChannel == null)
            {
                _foregroundChannel = _container.Resolve<IForegroundChannel>();
            }
            _foregroundChannel.OnUpdateFrameRate(frameRate);
        }
    }
}
