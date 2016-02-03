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

        public void SetupRenderer(uint foregroundProcessId, IMediaSource source)
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
            RenderFormatUpdate?.Invoke(0, width, height);
        }

        public void SetForegroundProcessId(uint processId)
        {
        }
    }
}
