using System;
using Windows.UI.Xaml.Media;
using ChatterBox.Client.Presentation.Shared.Controls;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class InstantMessageViewModel : IInstantMessage
    {
        public string Body { get; set; }
        public bool IsSender { get; set; }
        public DateTime DeliveredAt { get; set; }
        public ImageSource SenderProfileSource { get; set; }
        public string SenderName { get; set; }
    }
}