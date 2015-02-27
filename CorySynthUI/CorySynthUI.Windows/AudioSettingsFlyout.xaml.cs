using System;
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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace CorySynthUI
{
    public sealed partial class AudioSettingsFlyout : SettingsFlyout
    {
        public static readonly String CommandID = "09458439-B623-4143-9968-9B507B5E7075";


        public AudioSettingsFlyout()
        {
            this.InitializeComponent();
        }
    }
}
