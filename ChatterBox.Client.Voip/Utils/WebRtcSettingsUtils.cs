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

using ChatterBox.Client.Common.Settings;
using System;
using System.Collections.Generic;
using System.Text;

#if USE_WEBRTC_API
using RtcIceServer = webrtc_winrt_api.RTCIceServer;
#elif USE_ORTC_API
using RtcIceServer = ChatterBox.Client.Voip.Rtc.RTCIceServer;
#endif //USE_WEBRTC_API

namespace ChatterBox.Client.Voip.Utils
{
    internal class WebRtcSettingsUtils
    {
        public static List<RtcIceServer> ToRTCIceServer(IEnumerable<IceServer> iceServerList)
        {
            if (iceServerList == null) throw new ArgumentNullException(nameof(iceServerList));

            var rtcList = new List<RtcIceServer>();
            foreach (var iceServer in iceServerList)
            {
                rtcList.Add(new RtcIceServer
                {
                    Url = iceServer.Url,
                    Username = iceServer.Username ?? string.Empty,
                    Credential = iceServer.Password ?? string.Empty
                });
            }

            return rtcList;
        }
    }
}
