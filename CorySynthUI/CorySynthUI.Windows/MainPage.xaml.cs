using CorySynthUI.ViewModel;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
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
            this.TheKeyboard.KeyboardEvent += TheKeyboard_KeyboardEvent;
        }

        void TheKeyboard_KeyboardEvent(object sender, CorySignalGenerator.Sequencer.Midi.IMidiMessage message)
        {
            _viewModel.MidiChannel.ProcessMidiMessage(message);
        }

        private SequencerViewModel _viewModel;
        public SequencerViewModel ViewModel
        {
            get { return _viewModel; }
        }

        public void Init()
        {
            _viewModel = new SequencerViewModel(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2), (App.Current as App).DeviceModel);
       }

        private void NavLinkClick(object sender, RoutedEventArgs e)
        {
            var state = ((FrameworkElement)sender).Tag as String;
            VisualStateManager.GoToState(this, state, true);
        }

        private void VoicePanelNav_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, String.Format("{0}LayerSelected", (string)this.VoicePanelNav.SelectedValue), true);
        }

        

    }
}
