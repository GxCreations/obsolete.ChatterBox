using ChatterBox.Client.Common.Communication.Voip;
using ChatterBoxClient.Universal.BackgroundRenderer;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Universal.Background.Voip
{
    public sealed class VideoRenderHelper : IVideoRenderHelper
    {
        public VideoRenderHelper()
        {
            // Pipe the event
            _renderer.RenderFormatUpdate += (a, b, c) => RenderFormatUpdate(a, b, c);
        }

        public event ChatterBox.Client.Common.Communication.Voip.RenderFormatUpdateHandler RenderFormatUpdate;

        public void SetupRenderer(uint foregroundProcessId, IMediaSource source)
        {
            _renderer.SetupRenderer(foregroundProcessId, source);
            _isSetup = true;
        }

        public void Teardown()
        {
            _renderer.Teardown();
        }

        public bool IsRendererAlreadySetup()
        {
            return _isSetup;
        }

        public void SetMediaElement(CoreDispatcher dispatcher, MediaElement mediaElement)
        {
        }
        public void SetDisplaySize(Windows.Foundation.Size size)
        {
            _renderer.SetRenderControlSize(size);
        }

        private Renderer _renderer = new Renderer();
        bool _isSetup = false;
    }
}
