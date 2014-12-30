﻿using CorySynthUI.ViewModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CorySynthUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.Init();

            this.InitializeComponent();
        }

        private MainViewModel _viewModel;
        public MainViewModel ViewModel
        {
            get { return _viewModel; }
        }

        public void Init()
        {
            _viewModel = new MainViewModel(this.Dispatcher);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartPlaying();
            ViewModel.StartListening((Windows.Devices.Enumeration.DeviceInformation)this.MidiDevList.SelectedItem);
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StopPlaying();
        }
    }
}