using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using ChatterBox.Client.Presentation.Shared.Controls;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public sealed class CallStateToVisibilityConverter : IValueConverter
    {
        public bool Inverted { get; set; }


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var compareValue = (CallState)parameter;
            var booleanValue = (CallState)value == compareValue;
            return booleanValue
                ? (!Inverted ? Visibility.Visible : Visibility.Collapsed)
                : (!Inverted ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}