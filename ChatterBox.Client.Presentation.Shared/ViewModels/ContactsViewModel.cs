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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ChatterBox.Client.Common.Avatars;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using ChatterBox.Client.Presentation.Shared.Services;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class ContactsViewModel : BindableBase
    {
        private readonly Func<ConversationViewModel> _contactFactory;
        private ConversationViewModel _selectedConversation;

        public ContactsViewModel(IForegroundUpdateService foregroundUpdateService,
            Func<ConversationViewModel> contactFactory)
        {
            _contactFactory = contactFactory;
            foregroundUpdateService.OnPeerDataUpdated += OnPeerDataUpdated;
            foregroundUpdateService.GetShownUser += ForegroundUpdateService_GetShownUser;
            foregroundUpdateService.OnVoipStateUpdate += OnVoipStateUpdate;
            OnPeerDataUpdated();

            LayoutService.Instance.LayoutChanged += LayoutChanged;
            ShowSettings = new DelegateCommand(() => OnShowSettings?.Invoke());
        }

        public MediaElement RingtoneElement { get; set; }

        private void OnVoipStateUpdate(VoipState voipState)
        {
            Task.Run(async () =>
            {
                switch (voipState.State)
                {
                    case VoipStateEnum.LocalRinging:
                        await PlaySound(true);
                        break;
                    case VoipStateEnum.RemoteRinging:
                        await PlaySound(false);
                        break;
                    case VoipStateEnum.Idle:
                    case VoipStateEnum.EstablishOutgoing:
                    case VoipStateEnum.EstablishIncoming:
                        await StopSound();
                        break;
                    case VoipStateEnum.HangingUp:
                    case VoipStateEnum.ActiveCall:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

        }



        public async Task PlaySound(bool isIncomingCall)
        {
            if (RingtoneElement == null) return;
            var source = isIncomingCall
                ? "ms-appx:///Assets/Ringtones/IncomingCall.mp3"
                : "ms-appx:///Assets/Ringtones/OutgoingCall.mp3";

            await RingtoneElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RingtoneElement.Source = new Uri(source);
                RingtoneElement.Stop();
                RingtoneElement.Play();
            });
        }


        public async Task StopSound()
        {
            if (RingtoneElement == null) return;

            await RingtoneElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RingtoneElement.Stop();
                RingtoneElement.Source = null;
            });
        }


        private string ForegroundUpdateService_GetShownUser()
        {
            if (SelectedConversation != null)
            {
                return SelectedConversation.UserId;
            }
            return string.Empty;
        }

        public ObservableCollection<ConversationViewModel> Conversations { get; } =
            new ObservableCollection<ConversationViewModel>();



        public ConversationViewModel SelectedConversation
        {
            get { return _selectedConversation; }
            set
            {
                _selectedConversation?.OnNavigatedFrom();
                SetProperty(ref _selectedConversation, value);
                _selectedConversation?.OnNavigatedTo();
            }
        }

        public DelegateCommand ShowSettings { get; set; }

        private void Contact_OnCloseConversation(ConversationViewModel obj)
        {
            SelectedConversation = null;
        }

        private void LayoutChanged(LayoutType layout)
        {
            UpdateSelection();
        }

        private void OnPeerDataUpdated()
        {
            var peers = SignaledPeerData.Peers;
            ObservableCollection<ConversationViewModel> copyConversations = new ObservableCollection<ConversationViewModel>(Conversations);
            foreach (var contact in copyConversations)
            {
                if (peers.All(p => p.UserId != contact.UserId))
                {
                    Conversations.Remove(contact);
                }
            }
            foreach (var peer in peers)
            {
                var contact = Conversations.SingleOrDefault(s => s.UserId == peer.UserId);
                if (contact == null)
                {
                    contact = _contactFactory();
                    contact.Name = peer.Name;
                    contact.UserId = peer.UserId;
                    contact.ProfileSource = new BitmapImage(new Uri(AvatarLink.EmbeddedLinkFor(peer.Avatar)));
                    contact.OnCloseConversation += Contact_OnCloseConversation;
                    contact.OnIsInCallMode += Contact_OnIsInCallMode;
                    var sortList = Conversations.ToList();
                    sortList.Add(contact);
                    sortList = sortList.OrderBy(s => s.Name).ToList();
                    Conversations.Insert(sortList.IndexOf(contact), contact);
                    contact.Initialize();
                }
                contact.IsOnline = peer.IsOnline;
            }

            UpdateSelection();
        }

        public void ReloadPeerData()
        {
            OnPeerDataUpdated();
        }

        private void Contact_OnIsInCallMode(ConversationViewModel conversation)
        {
            SelectedConversation = conversation;
        }

        public event Action OnShowSettings;

        private void UpdateSelection()
        {
            if (SelectedConversation == null && LayoutService.Instance.LayoutType == LayoutType.Parallel)
            {
                SelectedConversation = Conversations.FirstOrDefault();
            }
        }

        public bool SelectConversation(string userId)
        {
            foreach (var conversation in Conversations)
            {
                if (conversation.UserId == userId)
                {
                    SelectedConversation = conversation;
                    return true;
                }
            }
            return false;
        }
    }
}
