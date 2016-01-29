using ChatterBox.Client.Common.Communication.Voip;
using System;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Win8dot1.Voip
{
    internal class VideoRenderHelper : IVideoRenderHelper
    {
#pragma warning disable 67 // The event 'RenderFormatUpdate' is never used

        // this event is not used in Win8.1 rendering part so
        // its warning is disabled
        public event RenderFormatUpdateHandler RenderFormatUpdate;

#pragma warning restore 67

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

        public bool IsRendererAlreadySetup()
        {
            return _isSetup;
        }
    }
}
