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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this.Dispatcher);
            this.Model.Init();

        }



        public MainViewModel Model { get { return this.DataContext as MainViewModel; } }


        public void Play()
        {
            IsPlaying = true;
            Model.StartPlaying();
            Model.StartListening(this.MidiDevicePicker.SelectedIndex);
            
        }

       
        public void Stop()
        {
            if(IsPlaying)
                Model.StopPlaying(); 

            IsPlaying = false;
            IsRecording = false;
            
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

        private void OpenReverbExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Model.LoadReverb();
        }

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

     
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
        }

        private void RecordExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Play();
        }
    }
}
