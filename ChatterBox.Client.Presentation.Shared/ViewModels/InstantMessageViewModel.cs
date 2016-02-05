using System;
using Windows.UI.Xaml.Media;
using ChatterBox.Client.Presentation.Shared.Controls;
using ChatterBox.Client.Presentation.Shared.MVVM;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class InstantMessageViewModel : BindableBase, IInstantMessage
    {
        public string Body { get; set; }
        public bool IsSender { get; set; }
        public DateTime DeliveredAt { get; set; }
        public ImageSource SenderProfileSource { get; set; }
        public string SenderName { get; set; }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetProperty(ref _isHighlighted, value); }
        }
    }
}