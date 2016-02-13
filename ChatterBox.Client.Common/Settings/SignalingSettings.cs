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

using Windows.Storage;

namespace ChatterBox.Client.Common.Settings
{
    public static class SignalingSettings
    {
        public static string SignalingServerHost
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(SignalingServerHost)))
                {
                    return (string)ApplicationData.Current.LocalSettings.Values[nameof(SignalingServerHost)];
                }
                SignalingServerHost = "localhost";
                return SignalingServerHost;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(SignalingServerHost), value); }
        }

        public static string SignalingServerPort
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(SignalingServerPort)))
                {
                    return (string) ApplicationData.Current.LocalSettings.Values[nameof(SignalingServerPort)];
                }
                SignalingServerPort = "50000";
                return SignalingServerPort;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(SignalingServerPort), value); }
        }

        public static bool AppInsightsEnabled
        {
            get {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(AppInsightsEnabled)))
                {
                    return (bool) ApplicationData.Current.LocalSettings.Values[nameof(AppInsightsEnabled)];
                }
                return false;
            }
            set { ApplicationData.Current.LocalSettings.Values.AddOrUpdate(nameof(AppInsightsEnabled), value); }
        }

    }
}