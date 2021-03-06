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

using ChatterBox.Client.Common.Communication.Voip;
using System;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VideoRenderHelper : IVideoRenderHelper
    {
        public event RenderFormatUpdateHandler RenderFormatUpdate;

        private CoreDispatcher _dispatcher;
        private MediaElement _mediaElement;
        bool _isSetup = false;

        public void SetMediaElement(CoreDispatcher dispatcher, MediaElement mediaElement)
        {
            _dispatcher = dispatcher;
            _mediaElement = mediaElement;
        }

        public void SetupRenderer(uint foregroundProcessId, IMediaSource source,
            Windows.Foundation.Size videoControlSize)
        {
            if (_mediaElement != null)
            {
                Action fn = (() =>
                {
                    _mediaElement.SetMediaStreamSource(source);
                });
                var asyncOp = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
                _isSetup = true;
            }
        }

        public void Teardown()
        {
        }

        public void SetDisplaySize(Windows.Foundation.Size size)
        {
        }

        public bool IsRendererAlreadySetup()
        {
            return _isSetup;
        }

        public void ResolutionChanged(uint width, uint height)
        {
            RenderFormatUpdate?.Invoke(0, width, height, 0);
        }
        public void UpdateForegroundProcessId(uint foregroundProcessId)
        {
        }
    }
}
