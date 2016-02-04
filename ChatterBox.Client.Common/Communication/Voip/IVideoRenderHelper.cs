using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public delegate void RenderFormatUpdateHandler(long swapChainHandle, uint width, uint height);

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
    }
}