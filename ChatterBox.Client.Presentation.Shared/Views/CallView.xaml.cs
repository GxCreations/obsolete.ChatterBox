using ChatterBox.Client.Presentation.Shared.Converters;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class CallView
    {
        private MediaElement _selfMediaElement = null;
        private MediaElement _peerMediaElement = null;
        private ConversationViewModel _oldConversationViewModel;

        public CallView()
        {
            InitializeComponent();
            SetVideoPresenters();
            DataContextChanged += CallView_DataContextChanged;
#if WIN10
            RegisterPropertyChangedCallback(VisibilityProperty, new DependencyPropertyChangedCallback((o, p) =>
            {
                SetLayout();
            }));
#endif
        }

        private void CallView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_oldConversationViewModel != null)
            {
                _oldConversationViewModel.RemoteNativeVideoSizeChanged -= ConversationViewModel_RemoteNativeVideoSizeChanged;
            }
            var conversationViewModel = DataContext as ConversationViewModel;
            conversationViewModel.RegisterAudioElement(SoundPlayElement);
            conversationViewModel.RegisterVideoElements(_selfMediaElement, _peerMediaElement);
            conversationViewModel.RemoteNativeVideoSizeChanged += ConversationViewModel_RemoteNativeVideoSizeChanged;
            _oldConversationViewModel = conversationViewModel;
        }

        private void ConversationViewModel_RemoteNativeVideoSizeChanged()
        {
            SetLayout();
        }

        private void SetVideoPresenters()
        {
            var boolToVisConverter = new BooleanToVisibilityConverter();

#if WIN10
            var peerSwapChainPanel = new WebRTCSwapChainPanel.WebRTCSwapChainPanel();

            var peerHandleBinding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("RemoteSwapChainPanelHandle"),
                Mode = BindingMode.OneWay
            };
            peerSwapChainPanel.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SwapChainPanelHandleProperty,
                peerHandleBinding);

            PeerVideoPresenter.Content = peerSwapChainPanel;

            var selfSwapChainPanel = new WebRTCSwapChainPanel.WebRTCSwapChainPanel();

            var selfHandleBinding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("LocalSwapChainPanelHandle"),
                Mode = BindingMode.OneWay
            };
            selfSwapChainPanel.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SwapChainPanelHandleProperty,
                selfHandleBinding);

            var selfSizeBinding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("LocalNativeVideoSize"),
            };
            selfSwapChainPanel.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SizeProperty,
                selfSizeBinding);

            SelfVideoPresenter.Content = selfSwapChainPanel;
#endif

#if WIN81
            _peerMediaElement = new MediaElement
            {
                RealTimePlayback = true
            };
            PeerVideoPresenter.Content = _peerMediaElement;

            _selfMediaElement = new MediaElement
            {
                RealTimePlayback = true
            };
            SelfVideoPresenter.Content = _selfMediaElement;            
#endif
        }

        private void SetLayout()
        {
            // set the size for peer placeholder
            PeerPlaceholder.Width = VideoGrid.ActualWidth;
            PeerPlaceholder.Height = VideoGrid.ActualHeight;

            var conversationViewModel = DataContext as ConversationViewModel;
            var remoteVideoSize = conversationViewModel.RemoteNativeVideoSize;

            // if remote size is bigger than current canvas size it will be centered
            // otherwise it is set on top left
            double leftMargin = 0;
            if (remoteVideoSize.Width > VideoGrid.ActualWidth)
            {
                leftMargin = (VideoGrid.ActualWidth - remoteVideoSize.Width) / 2f;
            }

            double topMargin = 0;
            if (remoteVideoSize.Height > VideoGrid.ActualHeight)
            {
                topMargin = (VideoGrid.ActualHeight - remoteVideoSize.Height) / 2f;
            }
            PeerVideoPresenter.Margin = new Thickness(leftMargin, topMargin, 0, 0);

            // set the size and position for self placeholder
            SelfBorder.Width = VideoGrid.ActualWidth * 0.25D;
            SelfBorder.Height = VideoGrid.ActualHeight * 0.25D;
        }

        private void VideoGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetLayout();
        }
    }
}
