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

namespace ChatterBox.Client.Common.Settings
{
    public sealed class MediaSettingsIds
    {
        public static string VideoDeviceSettings { get { return "VideoDeviceSettingsId"; } }

        public static string AudioDeviceSettings { get { return "AudioDeviceSettingsId"; } }

        public static string AudioPlayoutDeviceSettings { get { return "AudioDevicePlayoutSettingsId"; } }

        public static string VideoCodecSettings { get { return "VideoCodecSettingsId"; } }

        public static string AudioCodecSettings { get { return "AudioCodecSettingsId"; } }

        public static string PreferredVideoCaptureWidth { get { return "PreferredVideoCaptureWidth"; } }

        public static string PreferredVideoCaptureHeight { get { return "PreferredVideoCaptureHeight"; } }
        
        public static string PreferredVideoCaptureFrameRate { get { return "PreferredVideoCaptureFrameRate"; } }
    }
}
