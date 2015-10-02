﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ChatterBox.Client.Presentation.Shared.Views
{
    public sealed partial class CallView : UserControl
    {
        public CallView()
        {
            this.InitializeComponent();
        }

        private void VideoGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SelfPlaceholder.Width = SelfVideo.Width = e.NewSize.Width * 0.25D;
            SelfPlaceholder.Height = SelfVideo.Height = e.NewSize.Height * 0.25D;
        }
    }
}
