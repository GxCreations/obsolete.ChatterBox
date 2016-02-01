using Windows.UI.Xaml;

namespace ChatterBox.Client.Presentation.Shared.AttachedProperties
{
    public static class ActualSize
    {


        /// <summary>
        /// The ActualHeight attached property's name.
        /// </summary>
        public const string ActualHeightPropertyName = "ActualHeight";

        /// <summary>
        /// Gets the value of the ActualHeight attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the ActualHeight property of the specified object.</returns>
        public static double GetActualHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(ActualHeightProperty);
        }

        /// <summary>
        /// Sets the value of the ActualHeight attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the ActualHeight value of the specified object.</param>
        public static void SetActualHeight(DependencyObject obj, double value)
        {
            obj.SetValue(ActualHeightProperty, value);
        }

        /// <summary>
        /// Identifies the ActualHeight attached property.
        /// </summary>
        public static readonly DependencyProperty ActualHeightProperty = DependencyProperty.RegisterAttached(
            ActualHeightPropertyName,
            typeof(double),
            typeof(ActualSize),
            new PropertyMetadata(double.NaN));


        /// <summary>
        /// The ActualWidth attached property's name.
        /// </summary>
        public const string ActualWidthPropertyName = "ActualWidth";

        /// <summary>
        /// Gets the value of the ActualWidth attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the ActualWidth property of the specified object.</returns>
        public static double GetActualWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(ActualWidthProperty);
        }

        /// <summary>
        /// Sets the value of the ActualWidth attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the ActualWidth value of the specified object.</param>
        public static void SetActualWidth(DependencyObject obj, double value)
        {
            obj.SetValue(ActualWidthProperty, value);
        }

        /// <summary>
        /// Identifies the ActualWidth attached property.
        /// </summary>
        public static readonly DependencyProperty ActualWidthProperty = DependencyProperty.RegisterAttached(
            ActualWidthPropertyName,
            typeof(double),
            typeof(ActualSize),
            new PropertyMetadata(double.NaN));


        /// <summary>
        /// The Enabled attached property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        /// <summary>
        /// Gets the value of the Enabled attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the Enabled property of the specified object.</returns>
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        /// <summary>
        /// Sets the value of the Enabled attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the Enabled value of the specified object.</param>
        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        /// <summary>
        /// Identifies the Enabled attached property.
        /// </summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            EnabledPropertyName,
            typeof(bool),
            typeof(ActualSize),
            new PropertyMetadata(false, OnActualSizeEnable));

        private static void OnActualSizeEnable(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element == null) return;
            var enabled = (bool)e.NewValue;
            element.SizeChanged -= Element_SizeChanged;
            if (enabled)
            {
                element.SizeChanged += Element_SizeChanged;
            }

        }

        private static void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;
            SetActualHeight(element, e.NewSize.Height);
            SetActualWidth(element, e.NewSize.Height);
        }
    }
}
