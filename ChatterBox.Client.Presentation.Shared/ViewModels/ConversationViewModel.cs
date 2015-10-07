﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Settings;
using ChatterBox.Client.Signaling;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class ConversationViewModel : BindableBase
    {
        private readonly SignalingClient _signalingClient;
        private string _instantMessage;
        private bool _isOnline;
        private string _name;
        private ImageSource _profileSource;
        private string _userId;

        public ConversationViewModel(SignalingClient signalingClient, ISignalingUpdateService signalingUpdateService)
        {
            _signalingClient = signalingClient;
            signalingUpdateService.OnUpdate += SignalingUpdateService_OnUpdate;
            SendInstantMessageCommand = new DelegateCommand(OnSendInstantMessageCommandExecute,
                OnSendInstantMessageCommandCanExecute);
        }

        public string InstantMessage
        {
            get { return _instantMessage; }
            set
            {
                if (SetProperty(ref _instantMessage, value))
                {
                    SendInstantMessageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<InstantMessageViewModel> InstantMessages { get; } =
            new ObservableCollection<InstantMessageViewModel>();

        public bool IsOnline
        {
            get { return _isOnline; }
            set { SetProperty(ref _isOnline, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ImageSource ProfileSource
        {
            get { return _profileSource; }
            set { SetProperty(ref _profileSource, value); }
        }

        public DelegateCommand SendInstantMessageCommand { get; }

        public string UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }

        public void LoadHistory()
        {
            
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
                Payload = InstantMessage,
                Tag = RelayMessageTags.InstantMessage
            };
            InstantMessage = null;
            _signalingClient.Relay(message);
        }

        private void SignalingUpdateService_OnUpdate()
        {
            var newMessages = SignaledRelayMessages.Messages
                .Where(s => s.Tag == RelayMessageTags.InstantMessage && s.FromUserId == _userId).ToList();
            foreach (var message in newMessages)
            {
                InstantMessages.Add(new InstantMessageViewModel
                {
                    Message = message.Payload,
                    DateTime = message.SentDateTimeUtc,
                    Sender = Name,
                    IsSender = false
                });
                SignaledRelayMessages.Delete(message.Id.ToString());
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}