using ChatterBox.Client.Presentation.Shared.ViewModels;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class ContactsView
    {
        public ContactsView()
        {
            InitializeComponent();
            DataContextChanged += ContactsView_DataContextChanged;
            InputPane inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
        }

        private void ContactsView_DataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            var viewModel = DataContext as ContactsViewModel;
            if (viewModel == null) return;
            viewModel.RingtoneElement = RingtoneElement;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (Visibility != Visibility.Visible)
                return;

            if (MainGrid.RowDefinitions != null && MainGrid.RowDefinitions.Count >= 1)
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                MainGrid.InvalidateArrange();
            }
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (Visibility != Visibility.Visible)
                return;

            Rect coveredArea = sender.OccludedRect;
            var value = MainGrid.ActualHeight - coveredArea.Height;

            if (MainGrid.RowDefinitions != null &&
                MainGrid.RowDefinitions.Count >= 1 &&
                coveredArea.Height > 0 &&
                value > 0)
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(value);
                MainGrid.InvalidateArrange();
            }
        }
    }
}