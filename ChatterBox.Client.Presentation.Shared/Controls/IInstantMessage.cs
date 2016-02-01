using System;
using Windows.UI.Xaml.Media;

namespace ChatterBox.Client.Presentation.Shared.Controls
{
    public interface IInstantMessage
    {
        string Body { get; }
        bool IsSender { get; }
        DateTime DeliveredAt { get; }
        ImageSource SenderProfileSource { get; }
        string SenderName { get; }
    }
}