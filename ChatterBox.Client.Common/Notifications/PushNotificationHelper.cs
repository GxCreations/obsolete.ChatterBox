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
using System.Diagnostics;
using Windows.Networking.PushNotifications;
using ChatterBox.Client.Common.Settings;

namespace ChatterBox.Client.Common.Notifications
{
    public sealed class PushNotificationHelper
    {
        public static async void RegisterPushNotificationChannel()
        {
            bool registered = false;
            try
            {
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                if (channel != null)
                {
                    registered = true;
                    RegistrationSettings.PushNotificationChannelURI = channel.Uri;
                    Debug.WriteLine($"Push token chanell URI: {channel.Uri}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception during push token chanell URI reistration: {e.Message}");
            }

            if (!registered)
                Debug.WriteLine($"Push token chanell URI is NOT obtained");

        }
    }
}