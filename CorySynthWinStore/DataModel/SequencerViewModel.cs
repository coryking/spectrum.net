using CorySignalGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Sequencer;
using NAudio.Wave;
using CorySignalGenerator.Wave;
using WindowsPreview.Devices.Midi;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;

namespace CorySynthWinStore.Data
{
    public class SequencerViewModel : PropertyChangeModel, IDisposable
    {
        public const int Latency = 50;

        public SequencerViewModel(WaveFormat format)
        {
            MidiChannel = new Channel(0, format);

            WaveOut = new WaveOutPlayer(Latency);
            WaveOut.PlaybackStopped +=WaveOut_PlaybackStopped;

            MidiDevice = new MidiDevice();
            MidiDevice.MessageReceived += MidiDevice_MessageReceived;

        }

        
        #region Relay Commands

        private void InitalizeRelayCommands()
        {
            StartRecording = new RelayCommand(OnStartRecording, CanStartRecording);
            StopRecording = new RelayCommand(OnStopRecording, CanStopRecording);
        }

        public RelayCommand StartRecording { get; private set; }

        protected void OnStartRecording(object parameter)
        {
            WaveOut.StartPlayback(MidiChannel);
            IsRecording = true;
        }

        protected bool CanStartRecording(object paremeter)
        {
            return !IsRecording && WaveOut != null && SelectedAudioDevice != null;
        }

        public RelayCommand StopRecording { get; private set; }

        protected void OnStopRecording(object parameter)
        {
            WaveOut.EndPlayback();
        }

        protected bool CanStopRecording(object parameter)
        {
            return IsRecording && WaveOut != null && WaveOut.IsActive;
        }

        #endregion

        #region Event Handlers

        private void MidiDevice_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            Task.Run(() =>
            {
                MidiChannel.ProcessMidiMessage(CorySignalGenerator.Sequencer.Midi.MidiMessageConverter.ToMidiMessage(args.Message));
            });
        }


       

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsRecording = false;
        }

        #endregion

        #region Properties

        public DeviceInformation SelectedAudioDevice
        {
            get { return WaveOut.SelectedAudioDevice; }
        }

        public ObservableCollection<DeviceInformation> AudioDevices
        {
            get { return WaveOut.AudioDevices; }
        }


        #region Property IsRecording
        private bool _isRecording = false;

        public bool IsRecording
        {
            get
            {
                return _isRecording;
            }
            set
            {
                Set(ref _isRecording, value);
            }
        }
        #endregion

        public Channel MidiChannel { get; private set; }

        #region Midi Stuff

        public MidiDevice MidiDevice { get; private set; }

        #endregion

        protected WaveOutPlayer WaveOut { get; private set; }

        #endregion

        public void Dispose()
        {
            if (MidiDevice != null)
                MidiDevice.Dispose();
        }
    }
}
