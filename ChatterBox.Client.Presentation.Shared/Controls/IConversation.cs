﻿using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace ChatterBox.Client.Presentation.Shared.Controls
{
    public interface IConversation
    {
        string Name { get; }
        bool IsOnline { get; }
        bool IsSelfVideoAvailable { get; }
        bool IsPeerVideoAvailable { get; }
        bool IsMicrophoneEnabled { get; }
        bool IsVideoEnabled { get; }
        string InstantMessage { get; set; }
        IEnumerable<IInstantMessage> InstantMessages { get; }

        ImageSource ProfileSource { get; }
        ImageSource OwnProfileSource { get; }
        CallState CallState { get; }

        UIElement LocalVideoRenderer { get; }
        UIElement RemoteVideoRenderer { get; }

        ICommand AudioCallCommand { get; }
        ICommand VideoCallCommand { get; }
        ICommand AnswerCommand { get; }
        ICommand HangupCommand { get; }
        ICommand RejectCommand { get; }
        ICommand SwitchMicrophoneCommand { get; }
        ICommand SwitchVideoCommand { get; }
        ICommand SendInstantMessageCommand { get; }
        ICommand CloseConversationCommand { get; }
    }
}