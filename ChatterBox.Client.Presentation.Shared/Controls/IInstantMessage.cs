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
using Windows.UI.Xaml.Media;

namespace ChatterBox.Client.Presentation.Shared.Controls
{
    public interface IInstantMessage
    {
        string Body { get; }
        bool IsSender { get; }
        DateTime DeliveredAt { get; }
        ImageSource SenderProfileSource { get; }
        string SenderName { get; }
        bool IsHighlighted { get; set; }
    }
}