﻿using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class MainViewModel : DispatcherBindableBase
    {
        private bool _isActive;

        public MainViewModel(
            WelcomeViewModel welcomeViewModel,
            ConnectingViewModel connectingViewModel,
            ContactsViewModel contactsViewModel,
            SettingsViewModel settingsViewModel,
            CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
            WelcomeViewModel = welcomeViewModel;
            ConnectingViewModel = connectingViewModel;
            ContactsViewModel = contactsViewModel;
            SettingsViewModel = settingsViewModel;


            WelcomeViewModel.OnCompleted += WelcomeCompleted;
            ConnectingViewModel.ConnectionEstablished += ConnectingViewModel_ConnectionEstablished;
        }

        public ConnectingViewModel ConnectingViewModel { get; }
        public ContactsViewModel ContactsViewModel { get; }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public SettingsViewModel SettingsViewModel { get; }
        public WelcomeViewModel WelcomeViewModel { get; }

        private void ConnectingViewModel_ConnectionEstablished()
        {
            IsActive = true;
        }

        public void OnNavigatedTo()
        {
            IsActive = false;
            if (WelcomeViewModel.IsCompleted) WelcomeCompleted();
        }

        private void WelcomeCompleted()
        {
            ConnectingViewModel.EstablishConnection();
        }
    }
}