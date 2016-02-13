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

using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public delegate void RenderFormatUpdateHandler(long swapChainHandle, uint width, uint height, uint foregroundProcessId);

    public interface IVideoRenderHelper
    {
        event RenderFormatUpdateHandler RenderFormatUpdate;
        void SetupRenderer(uint foregroundProcessId, IMediaSource source,
            Windows.Foundation.Size videoControlSize);
        void Teardown();
        bool IsRendererAlreadySetup();

        void SetMediaElement(CoreDispatcher dispatcher, MediaElement mediaElement);
        void SetDisplaySize(Windows.Foundation.Size size);
        void ResolutionChanged(uint width, uint height);
        void UpdateForegroundProcessId(uint foregroundProcessId);
    }
}