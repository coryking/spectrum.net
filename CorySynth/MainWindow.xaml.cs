using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using NAudio.Midi;
using System.Windows.Threading;

namespace CorySignalGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        private WasapiOut waveOut;
        private MidiIn midiIn;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            this.Model.SignalPathChanged +=Model_SignalPathChanged;
        }

        private void Model_SignalPathChanged(object sender)
        {
            if (IsPlaying && waveOut != null)
            {
                waveOut.Init(Model.GetAudioChain());
            }
        }



        public MainWindowViewModel Model { get { return this.DataContext as MainWindowViewModel; } }


        public void Play()
        {
            IsPlaying = true;
            GenerateAudioChain();
            Model.Start();

        }

        public void RecordMidi()
        {
            if (midiIn == null)
            {
                midiIn = new MidiIn(this.MidiDevicePicker.SelectedIndex); ;
                midiIn.MessageReceived += midiIn_MessageReceived;
            }
            GenerateAudioChain();
            midiIn.Start();
            IsRecording = true;
            
        }

        private void GenerateAudioChain()
        {
            if (waveOut == null)
            {
                waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 5);
                waveOut.PlaybackStopped += waveOut_PlaybackStopped;

                waveOut.Init(Model.GetAudioChain());
                waveOut.Play();

            }
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // Ignore clock events
            if (e.MidiEvent.CommandCode == MidiCommandCode.TimingClock || e.MidiEvent.CommandCode == MidiCommandCode.AutoSensing)
                return;
            Console.WriteLine(" > Got Midi Event, Invoking Main Thread! {0}", e.MidiEvent);
            this.Dispatcher.Invoke(() => { Model.PlayNote(e.MidiEvent); });
            //Model.PlayNote(e.MidiEvent);
        
        }

        void waveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            DisposeAudioDevices();
        }

        private void DisposeAudioDevices()
        {
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
            if (midiIn != null)
            {
                midiIn.Dispose();
                midiIn = null;
            }
        }

        public void Stop()
        {
            if(IsPlaying)
                Model.Stop(); 

            IsPlaying = false;
            IsRecording = false;
            
            if(waveOut != null)
                waveOut.Stop();

            if (midiIn != null)
                midiIn.Stop();
        }


        #region IsRecording

        /// <summary>
        /// Identifies the <see cref="IsRecording"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register("IsRecording", typeof(bool), typeof(MainWindow),
                new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the IsRecording property.  This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsRecording
        {
            get { return (bool)GetValue(IsRecordingProperty); }
            set { SetValue(IsRecordingProperty, value); }
        }

        #endregion

        #region IsPlaying

        /// <summary>
        /// Identifies the <see cref="IsPlaying"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(MainWindow),
                new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the IsPlaying property.  This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        #endregion

        private void CanPlay(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsPlaying && !IsRecording;
        }

        private void PlayExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Play();
        }

        private void CanStop(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsPlaying || IsRecording;
        }

        private void StopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Stop();
        }

        private void CanRecord(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsPlaying && (this.MidiDevicePicker != null && this.MidiDevicePicker.SelectedIndex > -1);

        }

        private void RecordExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            RecordMidi();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
        }
    }
}
