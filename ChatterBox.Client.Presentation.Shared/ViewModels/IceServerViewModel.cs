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

using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class IceServerViewModel : BindableBase
    {
        public IceServerViewModel(IceServer iceServer)
        {
            IceServer = iceServer;

            Url = iceServer.Url;
            Username = iceServer.Username;
            Password = iceServer.Password;
        }

        public bool Apply()
        {
            IceServer.Url = Url;
            IceServer.Username = Username;
            IceServer.Password = Password;

            return !string.IsNullOrEmpty(Url);
        }

        public IceServer IceServer { get; set; }    

        protected string _url;
        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }        
    }
}

