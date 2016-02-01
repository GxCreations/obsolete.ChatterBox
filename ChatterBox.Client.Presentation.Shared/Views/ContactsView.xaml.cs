using ChatterBox.Client.Presentation.Shared.ViewModels;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class ContactsView
    {
        public ContactsView()
        {
            InitializeComponent();
            DataContextChanged += ContactsView_DataContextChanged;
        }

        private void ContactsView_DataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            var viewModel = DataContext as ContactsViewModel;
            if(viewModel==null) return;
            viewModel.RingtoneElement = RingtoneElement;
        }
    }
}