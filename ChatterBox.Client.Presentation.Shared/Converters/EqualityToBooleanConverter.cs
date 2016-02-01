using System;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public sealed class EqualityToBooleanConverter : IValueConverter
    {
        public bool Inverted { get; set; }


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var booleanValue = value.Equals(parameter);
            return booleanValue ? !Inverted : Inverted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}