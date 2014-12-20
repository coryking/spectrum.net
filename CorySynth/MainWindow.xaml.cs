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

namespace CorySynth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WasapiOut waveOut;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        public MainWindowViewModel Model { get { return this.DataContext as MainWindowViewModel; } }


        public void Play()
        {
            IsPlaying = true;
            GenerateAudioChain();
        }

        private void GenerateAudioChain()
        {
            if (waveOut == null)
            {
                waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
                waveOut.PlaybackStopped += waveOut_PlaybackStopped;

                waveOut.Init(Model.GetAudioChain());
                waveOut.Play();
            }
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
        }

        public void Stop()
        {
            IsPlaying = false;
            if(waveOut != null)
                waveOut.Stop();
        }

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
            e.CanExecute = !IsPlaying;
        }

        private void PlayExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Play();
        }

        private void CanStop(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsPlaying;
        }

        private void StopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Stop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
        }
    }
}
