﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Windows.UI.Core;
using ChatterBox.Client.Common.Signaling.PersistedData;
using ChatterBox.Client.Presentation.Shared.MVVM;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public sealed class MainViewModel : DispatcherBindableBase
    {
        private bool _isActive;
        private bool _isSettingsVisible;

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
            ConnectingViewModel.OnRegistered += ConnectingViewModel_OnRegistered;
            ConnectingViewModel.OnRegistrationFailed += ConnectingViewModel_OnRegistrationFailed;
            ShowSettingsCommand = new DelegateCommand(() => IsSettingsVisible = true);

            WelcomeViewModel.OnShowSettings += () => IsSettingsVisible = true;
            ContactsViewModel.OnShowSettings += () => IsSettingsVisible = true;
            ConnectingViewModel.OnShowSettings += () => IsSettingsVisible = true;
            SettingsViewModel.OnClose += SettingsViewModelOnClose;
            SettingsViewModel.OnRegistrationSettingsChanged += RegistrationSettingChanged;
        }        

        public ConnectingViewModel ConnectingViewModel { get; }
        public ContactsViewModel ContactsViewModel { get; }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public bool IsSettingsVisible
        {
            get { return _isSettingsVisible; }
            set
            {                
                SetProperty(ref _isSettingsVisible, value);
                if (value) SettingsViewModel.OnNavigatedTo();
                else SettingsViewModel.OnNavigatedFrom();
            }
        }

        public SettingsViewModel SettingsViewModel { get; }
        public DelegateCommand ShowSettingsCommand { get; set; }
        public WelcomeViewModel WelcomeViewModel { get; }

        private void ConnectingViewModel_OnRegistered()
        {
            IsActive = true;
        }

        private void ConnectingViewModel_OnRegistrationFailed()
        {
            IsActive = false;
        }

        public void OnNavigatedTo()
        {
            if (WelcomeViewModel.IsCompleted) WelcomeCompleted();
            if (SignalingStatus.IsRegistered)
            {
                //_signalingProxy.GetPeerList(new Message());
            }
        }

        private void SettingsViewModelOnClose()
        {
            IsSettingsVisible = false;
        }

        private void RegistrationSettingChanged()
        {
            IsActive = false;
            ConnectingViewModel.SwitchSignalingServer();
        }

        private void WelcomeCompleted()
        {
            ApplicationView.GetForCurrentView().Title = WelcomeViewModel.Name;
            ConnectingViewModel.EstablishConnection();
        }
    }
}