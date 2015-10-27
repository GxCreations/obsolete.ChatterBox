﻿using System;
using System.Linq;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class WelcomeViewModel : BindableBase
    {
        private string _domain;
        private bool _isCompleted;
        private string _name;

        public WelcomeViewModel()
        {
            CompleteCommand = new DelegateCommand(OnCompleteCommandExecute, CanCompleteCommandExecute);

            Domain = RegistrationSettings.Domain;
            Name = RegistrationSettings.Name;

            IsCompleted = ValidateStrings(Name, Domain);
        }

        public DelegateCommand CompleteCommand { get; }

        public string Domain
        {
            get { return _domain; }
            set
            {
                if (SetProperty(ref _domain, value))
                {
                    CompleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set { SetProperty(ref _isCompleted, value); }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    CompleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool CanCompleteCommandExecute()
        {
            return ValidateStrings(Name, Domain);
        }

        private void OnCompleteCommandExecute()
        {
            RegistrationSettings.Name = Name;
            RegistrationSettings.Domain = Domain;
            IsCompleted = true;
            OnCompleted?.Invoke();
        }

        public event Action OnCompleted;

        private bool ValidateStrings(params string[] strings)
        {
            return strings != null &&
                   strings.All(@string => !string.IsNullOrWhiteSpace(@string) && !string.IsNullOrEmpty(@string));
        }
    }
}