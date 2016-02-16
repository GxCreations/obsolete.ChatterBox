using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ChatterBox.Client.Presentation.Shared.Converters
{
    public class BooleanToStyleConverter : IValueConverter
    {
        public Style FalseStyle { get; set; }
        public Style TrueStyle { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var newState = (bool)value;
            return newState ? TrueStyle : FalseStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}