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
            peerSwapChainPanel.SizeChanged += PeerSwapChainPanel_SizeChanged;
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
            selfSwapChainPanel.SizeChanged += SelfSwapChainPanel_SizeChanged;
            SelfVideoPresenter.Content = selfSwapChainPanel;
#endif

#if WIN81
            _peerMediaElement = new MediaElement
            {
                RealTimePlayback = true
            };
            peerSwapChainPanel.SizeChanged += PeerSwapChainPanel_SizeChanged;
            PeerVideoPresenter.Content = _peerMediaElement;

            _selfMediaElement = new MediaElement
            {
                RealTimePlayback = true
            };
            selfSwapChainPanel.SizeChanged += SelfSwapChainPanel_SizeChanged;
            SelfVideoPresenter.Content = _selfMediaElement;            
#endif
        }

        private void SelfSwapChainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var conversationViewModel = DataContext as ConversationViewModel;
            conversationViewModel.LocalVideoControlSize = e.NewSize;
        }

        private void PeerSwapChainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var conversationViewModel = DataContext as ConversationViewModel;
            conversationViewModel.RemoteVideoControlSize = e.NewSize;
        }

        private void SetLayout()
        {
            // set the size for peer placeholder
            PeerPlaceholder.Width = VideoGrid.ActualWidth;
            PeerPlaceholder.Height = VideoGrid.ActualHeight;

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
