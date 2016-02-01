using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Common.Communication.Contracts;
using ChatterBox.Common.Communication.Messages.Relay;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;
using Windows.UI.Xaml;
using ChatterBox.Client.Presentation.Shared.Controls;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConversationViewModel : BindableBase, IDisposable, IConversation
    {
        public ICommand AudioCallCommand { get; }
        public ICommand VideoCallCommand { get; }
        public ICommand AnswerCommand { get; }
        public ICommand HangupCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand SwitchMicrophoneCommand { get; }
        public ICommand SwitchVideoCommand { get; }
        public ICommand SendInstantMessageCommand { get; }



        private readonly IClientChannel _clientChannel;
        private readonly IVoipChannel _voipChannel;
        private readonly IForegroundUpdateService _foregroundUpdateService;
        private string _instantMessage;
        private bool _isOnline;
        private bool _isOtherConversationInCallMode;
        private string _name;
        private ImageSource _profileSource;
        private string _userId;
        private long _localSwapChainHandle;
        private long _remoteSwapChainHandle;
        private Size _localVideoControlSize;
        private Size _remoteVideoControlSize;
        private bool _isMicEnabled;
        private bool _isVideoEnabled;
        private bool _canCloseConversation;


        public ConversationViewModel(IClientChannel clientChannel,
                                     IForegroundUpdateService foregroundUpdateService,
                                     IVoipChannel voipChannel)
        {
            _clientChannel = clientChannel;
            _voipChannel = voipChannel;
            _foregroundUpdateService = foregroundUpdateService;
            foregroundUpdateService.OnRelayMessagesUpdated += OnRelayMessagesUpdated;
            foregroundUpdateService.OnVoipStateUpdate += OnVoipStateUpdate;
            foregroundUpdateService.OnFrameFormatUpdate += OnFrameFormatUpdate;
            SendInstantMessageCommand = new DelegateCommand(OnSendInstantMessageCommandExecute,
                OnSendInstantMessageCommandCanExecute);
            AudioCallCommand = new DelegateCommand(OnCallCommandExecute, OnCallCommandCanExecute);
            VideoCallCommand = new DelegateCommand(OnVideoCallCommandExecute, OnVideoCallCommandCanExecute);
            HangupCommand = new DelegateCommand(OnHangupCommandExecute, OnHangupCommandCanExecute);
            AnswerCommand = new DelegateCommand(OnAnswerCommandExecute, OnAnswerCommandCanExecute);
            RejectCommand = new DelegateCommand(OnRejectCommandExecute, OnRejectCommandCanExecute);
            CloseConversationCommand = new DelegateCommand(OnCloseConversationCommandExecute, () => _canCloseConversation);
            SwitchMicrophoneCommand = new DelegateCommand(SwitchMicCommandExecute, SwitchMicCommandCanExecute);
            SwitchVideoCommand = new DelegateCommand(SwitchVideoCommandExecute, SwitchVideoCommandCanExecute);
            LayoutService.Instance.LayoutChanged += LayoutChanged;
            LayoutChanged(LayoutService.Instance.LayoutType);
            SetVideoPresenters();
        }

        private void LayoutChanged(LayoutType state)
        {
            _canCloseConversation = state == LayoutType.Overlay;
            ((DelegateCommand)CloseConversationCommand).RaiseCanExecuteChanged();
        }

        internal void OnNavigatedTo()
        {
        }

        internal void OnNavigatedFrom()
        {
        }


        public ICommand CloseConversationCommand { get; }

        public string InstantMessage
        {
            get { return _instantMessage; }
            set
            {
                if (SetProperty(ref _instantMessage, value))
                {
                    ((DelegateCommand)SendInstantMessageCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<IInstantMessage> InstantMessages { get; } =
            new ObservableCollection<InstantMessageViewModel>();

        private void SetVideoPresenters()
        {
#if WIN10
            var remoteVideoRenderer = new WebRTCSwapChainPanel.WebRTCSwapChainPanel();
            remoteVideoRenderer.SizeChanged += (s, e) => { RemoteVideoControlSize = e.NewSize; };

            remoteVideoRenderer.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SwapChainPanelHandleProperty,
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(nameof(RemoteSwapChainPanelHandle)),
                    Mode = BindingMode.OneWay
                });
            RemoteVideoRenderer = remoteVideoRenderer;

            var localVideoRenderer = new WebRTCSwapChainPanel.WebRTCSwapChainPanel();
            localVideoRenderer.SizeChanged += (s, e) => { LocalVideoControlSize = e.NewSize; };

            localVideoRenderer.SetBinding(
                WebRTCSwapChainPanel.WebRTCSwapChainPanel.SwapChainPanelHandleProperty,
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(nameof(LocalSwapChainPanelHandle)),
                    Mode = BindingMode.OneWay
                });
            LocalVideoRenderer = localVideoRenderer;
#endif

#if WIN81
            RemoteVideoRenderer = new MediaElement
            {
                RealTimePlayback = true
            };

            LocalVideoRenderer = new MediaElement
            {
                RealTimePlayback = true
            };
#endif
        }

        public bool IsOnline
        {
            get { return _isOnline; }
            set { SetProperty(ref _isOnline, value); }
        }

        public CallState CallState
        {
            get { return _callState; }
            set
            {
                if (!SetProperty(ref _callState, value)) return;
                if (value != CallState.Idle) OnIsInCallMode?.Invoke(this);
                UpdateCommandStates();
            }
        }

        private UIElement _localVideoRenderer;
        private UIElement _remoteVideoRenderer;
        public UIElement LocalVideoRenderer
        {
            get
            {
                return _localVideoRenderer;
            }
            set
            {
                SetProperty(ref _localVideoRenderer, value);
            }
        }

        public UIElement RemoteVideoRenderer
        {
            get
            {
                return _remoteVideoRenderer;
            }
            set
            {
                SetProperty(ref _remoteVideoRenderer, value);
            }
        }

        public bool IsOtherConversationInCallMode
        {
            get { return _isOtherConversationInCallMode; }
            set
            {
                if (SetProperty(ref _isOtherConversationInCallMode, value))
                    UpdateCommandStates();
            }
        }


        private bool _isPeerVideoAvailable;
        public bool IsPeerVideoAvailable
        {
            get { return _isPeerVideoAvailable; }
            set { SetProperty(ref _isPeerVideoAvailable, value); }
        }

        private bool _isSelfVideoAvailable;

        public bool IsSelfVideoAvailable
        {
            get { return _isSelfVideoAvailable; }
            set { SetProperty(ref _isSelfVideoAvailable, value); }
        }

        private bool _isAudioOnlyCall;

        public bool IsAudioOnlyCall
        {
            get { return _isAudioOnlyCall; }
            set
            {
                SetProperty(ref _isAudioOnlyCall, value);
                UpdateShowVideoButtonFlags();
            }
        }


        private bool _showVideoOnButton;
        public bool ShowVideoOnButton
        {
            get { return _showVideoOnButton; }
            set { SetProperty(ref _showVideoOnButton, value); }
        }

        private bool _showVideoOffButton;
        private CallState _callState;

        public bool ShowVideoOffButton
        {
            get { return _showVideoOffButton; }
            set { SetProperty(ref _showVideoOffButton, value); }
        }

        private void UpdateShowVideoButtonFlags()
        {
            ShowVideoOnButton = !IsAudioOnlyCall && IsVideoEnabled;
            ShowVideoOffButton = !IsAudioOnlyCall && !IsVideoEnabled;
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ImageSource OwnProfileSource { get; } = new BitmapImage(new Uri(AvatarLink.EmbeddedLinkFor(SignalingStatus.Avatar)));

        public ImageSource ProfileSource
        {
            get { return _profileSource; }
            set { SetProperty(ref _profileSource, value); }
        }


        public string UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }

        public long LocalSwapChainPanelHandle
        {
            get
            {
                return _localSwapChainHandle;
            }
            set
            {
                IsSelfVideoAvailable = value > 0;
                _localSwapChainHandle = value;
                // Don't use SetProperty() because it does nothing if the value
                // doesn't change but in this case it must always update the
                // swap chain panel.
                OnPropertyChanged(nameof(LocalSwapChainPanelHandle));
            }
        }

        public long RemoteSwapChainPanelHandle
        {
            get
            {
                return _remoteSwapChainHandle;
            }
            set
            {
                IsPeerVideoAvailable = value > 0;
                _remoteSwapChainHandle = value;
                // Don't use SetProperty() because it does nothing if the value
                // doesn't change but in this case it must always update the
                // swap chain panel.
                OnPropertyChanged(nameof(RemoteSwapChainPanelHandle));
            }
        }

        public Size LocalVideoControlSize
        {
            get
            {
                return _localVideoControlSize;
            }
            set
            {
                if (SetProperty(ref _localVideoControlSize, value))
                {
                    _voipChannel.OnLocalControlSize(value);
                }
            }
        }

        public event Action RemoteNativeVideoSizeChanged;

        public Size RemoteVideoControlSize
        {
            get
            {
                return _remoteVideoControlSize;
            }
            set
            {
                if (SetProperty(ref _remoteVideoControlSize, value))
                {
                    RemoteNativeVideoSizeChanged?.Invoke();
                    _voipChannel.OnRemoteControlSize(value);
                }
            }
        }

        public bool IsMicrophoneEnabled
        {
            get
            {
                return _isMicEnabled;
            }
            set
            {
                SetProperty(ref _isMicEnabled, value);
            }
        }

        public bool IsVideoEnabled
        {
            get
            {
                return _isVideoEnabled;
            }
            set
            {
                SetProperty(ref _isVideoEnabled, value);
                UpdateShowVideoButtonFlags();
            }
        }



        public void Initialize()
        {
            var voipState = _voipChannel.GetVoipState();
            if (voipState != null)
            {
                OnVoipStateUpdate(voipState);
            }

            //Get stored relay messages
            OnRelayMessagesUpdated();
        }

        private bool OnAnswerCommandCanExecute()
        {
            return CallState == CallState.LocalRinging;
        }

        private void OnAnswerCommandExecute()
        {
            _voipChannel.RegisterVideoElements(LocalVideoRenderer as MediaElement, RemoteVideoRenderer as MediaElement);
            _voipChannel.Answer();
        }

        private bool OnVideoCallCommandCanExecute()
        {
            return (CallState == CallState.Idle) && !IsOtherConversationInCallMode;
        }

        private void OnVideoCallCommandExecute()
        {
            IsAudioOnlyCall = false;
            _voipChannel.RegisterVideoElements(LocalVideoRenderer as MediaElement, RemoteVideoRenderer as MediaElement);
            _voipChannel.Call(new OutgoingCallRequest
            {
                PeerUserId = UserId,
                VideoEnabled = true
            });
        }

        private bool OnCallCommandCanExecute()
        {
            return (CallState == CallState.Idle) && !IsOtherConversationInCallMode;
        }

        private void OnCallCommandExecute()
        {
            IsAudioOnlyCall = true;
            _voipChannel.Call(new OutgoingCallRequest
            {
                PeerUserId = UserId,
                VideoEnabled = false
            });
            IsSelfVideoAvailable = false;
        }

        public event Action<ConversationViewModel> OnCloseConversation;

        private void OnCloseConversationCommandExecute()
        {
            OnCloseConversation?.Invoke(this);
        }

        private bool OnHangupCommandCanExecute()
        {
            return (CallState == CallState.Connected) || (CallState == CallState.RemoteRinging);
        }

        private void OnHangupCommandExecute()
        {
            _voipChannel.Hangup();
        }

        private bool OnRejectCommandCanExecute()
        {
            return CallState == CallState.LocalRinging;
        }

        private void OnRejectCommandExecute()
        {
            _voipChannel.Reject(new IncomingCallReject
            {
                Reason = "Rejected"
            });
        }

        private bool SwitchMicCommandCanExecute()
        {
            return CallState != CallState.Idle;
        }

        private void SwitchMicCommandExecute()
        {
            IsMicrophoneEnabled = !IsMicrophoneEnabled;
            _voipChannel.ConfigureMicrophone(new MicrophoneConfig
            {
                Muted = !IsMicrophoneEnabled
            });
        }

        private bool SwitchVideoCommandCanExecute()
        {
            return CallState != CallState.Idle;
        }

        private void SwitchVideoCommandExecute()
        {
            IsVideoEnabled = !IsVideoEnabled;
            _voipChannel.ConfigureVideo(new VideoConfig
            {
                On = IsVideoEnabled
            });
            IsSelfVideoAvailable = IsVideoEnabled;
        }

        private void OnRelayMessagesUpdated()
        {
            var newMessages = SignaledRelayMessages.Messages
                .Where(s => s.Tag == RelayMessageTags.InstantMessage && s.FromUserId == _userId)
                .OrderBy(s => s.SentDateTimeUtc).ToList();

            foreach (var message in newMessages)
            {
                ((ObservableCollection<InstantMessageViewModel>)InstantMessages).Add(new InstantMessageViewModel
                {
                    Body = message.Payload,
                    DeliveredAt = message.SentDateTimeUtc.LocalDateTime,
                    IsSender = false,
                    SenderName = Name,
                    SenderProfileSource = ProfileSource
                });
                SignaledRelayMessages.Delete(message.Id);
            }
        }

        private bool OnSendInstantMessageCommandCanExecute()
        {
            return !string.IsNullOrWhiteSpace(InstantMessage);
        }

        private void OnSendInstantMessageCommandExecute()
        {
            var message = new RelayMessage
            {
                SentDateTimeUtc = DateTimeOffset.UtcNow,
                ToUserId = UserId,
                FromUserId = RegistrationSettings.UserId,
                Payload = InstantMessage.Trim(),
                Tag = RelayMessageTags.InstantMessage
            };
            InstantMessage = null;
            _clientChannel.Relay(message);
            ((ObservableCollection<InstantMessageViewModel>)InstantMessages).Add(new InstantMessageViewModel
            {
                Body = message.Payload,
                DeliveredAt = message.SentDateTimeUtc.LocalDateTime,
                IsSender = true,
                SenderName = RegistrationSettings.Name,
                SenderProfileSource = OwnProfileSource
            });
        }

        private void OnVoipStateUpdate(VoipState voipState)
        {
            switch (voipState.State)
            {
                case VoipStateEnum.Idle:
                    CallState = CallState.Idle;
                    IsOtherConversationInCallMode = false;
                    LocalSwapChainPanelHandle = 0;
                    RemoteSwapChainPanelHandle = 0;
                    break;
                case VoipStateEnum.LocalRinging:
                    if (voipState.PeerId == UserId)
                    {
                        CallState = CallState.LocalRinging;
                        IsMicrophoneEnabled = true; //Start new calls with mic enabled
                        IsVideoEnabled = voipState.IsVideoEnabled;
                        IsAudioOnlyCall = !voipState.IsVideoEnabled;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.RemoteRinging:
                    if (voipState.PeerId == UserId)
                    {
                        CallState = CallState.RemoteRinging;
                        IsMicrophoneEnabled = true; //Start new calls with mic enabled
                        IsVideoEnabled = voipState.IsVideoEnabled;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.EstablishOutgoing:
                    if (voipState.PeerId == UserId)
                    {
                        CallState = CallState.Connected;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.EstablishIncoming:
                    if (voipState.PeerId == UserId)
                    {
                        CallState = CallState.Connected;
                        IsSelfVideoAvailable = IsVideoEnabled;
                        IsPeerVideoAvailable = voipState.IsVideoEnabled;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.HangingUp:
                    if (voipState.PeerId == UserId)
                    {
                        CallState = CallState.Connected;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                case VoipStateEnum.ActiveCall:
                    if (voipState.PeerId == UserId)
                    {
                        CallState = CallState.Connected;
                        IsSelfVideoAvailable = IsVideoEnabled;
                        IsPeerVideoAvailable = voipState.IsVideoEnabled;
                    }
                    else
                    {
                        IsOtherConversationInCallMode = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void OnFrameFormatUpdate(FrameFormat obj)
        {
            if (CallState == CallState.Idle)
            {
                return;
            }

            if (obj.IsLocal)
            {
                LocalSwapChainPanelHandle = obj.SwapChainHandle;
            }
            else
            {
                RemoteSwapChainPanelHandle = obj.SwapChainHandle;
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        private void UpdateCommandStates()
        {
            ((DelegateCommand)AudioCallCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)VideoCallCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AnswerCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)HangupCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)RejectCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SwitchMicrophoneCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SwitchVideoCommand).RaiseCanExecuteChanged();
        }

        public event Action<ConversationViewModel> OnIsInCallMode;









        // Avoid memory leak by unsubscribing from foregroundUpdateService object
        // because its lifetime may be much longer.
        public void Dispose()
        {
            if (_foregroundUpdateService == null) return;

            _foregroundUpdateService.OnRelayMessagesUpdated -= OnRelayMessagesUpdated;
            _foregroundUpdateService.OnVoipStateUpdate -= OnVoipStateUpdate;
            _foregroundUpdateService.OnFrameFormatUpdate -= OnFrameFormatUpdate;
        }


    }
}
