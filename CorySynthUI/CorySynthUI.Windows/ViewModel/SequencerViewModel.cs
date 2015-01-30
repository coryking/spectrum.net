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
using CorySignalGenerator.Sounds;
using CorySignalGenerator.Sequencer.Interfaces;

namespace CorySynthUI.ViewModel
{
    public class SequencerViewModel : PropertyChangeModel, IDisposable
    {
        public const int Latency = 50;

        public SequencerViewModel(WaveFormat format)
        {
            MidiChannel = new Channel(0, format);

            WaveOut = new WaveOutPlayer(Latency);
            WaveOut.PlaybackStopped +=WaveOut_PlaybackStopped;
            WaveOut.PropertyChanged += WaveOut_PropertyChanged;

            MidiDevice = new MidiDevice();
            MidiDevice.MessageReceived += MidiDevice_MessageReceived;

            InitalizeRelayCommands();
        }

        void WaveOut_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (StartRecordingCommand != null)
                StartRecordingCommand.RaiseCanExecuteChanged();
            if (StopRecordingCommand != null)
                StopRecordingCommand.RaiseCanExecuteChanged();
        }

        
        #region Relay Commands

        private void InitalizeRelayCommands()
        {
            StartRecordingCommand = new RelayCommand(OnStartRecording, CanStartRecording);
            StopRecordingCommand = new RelayCommand(OnStopRecording, CanStopRecording);
        }

        public RelayCommand StartRecordingCommand { get; private set; }

        protected void OnStartRecording(object parameter)
        {
            WaveOut.StartPlayback(MidiChannel);
            IsRecording = true;
        }

        protected bool CanStartRecording(object paremeter)
        {
            return !IsRecording && WaveOut != null && SelectedAudioDevice != null;
        }

        public RelayCommand StopRecordingCommand { get; private set; }

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
                var msg = CorySignalGenerator.Sequencer.Midi.MidiMessageConverter.ToMidiMessage(args.Message);
                if(msg != null)
                    MidiChannel.ProcessMidiMessage(msg);
            });
        }


       

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsRecording = false;
        }

        #endregion

        #region Properties


        
        #region Property SelectedVoice
        private IVoice _selectedVoice = null;

        /// <summary>
        /// Gets / Sets the selected voice
        /// </summary>
        public IVoice SelectedVoice
        {
            get
            {
                return _selectedVoice;
            }
            set
            {
                Set(ref _selectedVoice, value);
            }
        }
        #endregion
		
		


        public DeviceInformation SelectedAudioDevice
        {
            get { return WaveOut.SelectedAudioDevice; }
        }

        public ObservableCollection<DeviceInformation> AudioDevices
        {
            get { return WaveOut.AudioDevices; }
        }

      
        public ObservableCollection<DeviceInformation> MidiDevices
        {
            get { return MidiDevice.MidiDevices; }
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
                StartRecordingCommand.RaiseCanExecuteChanged();
                StopRecordingCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        public Channel MidiChannel { get; private set; }

        #region Midi Stuff

        public MidiDevice MidiDevice { get; private set; }

        #endregion

        public WaveOutPlayer WaveOut { get; private set; }

        #endregion

        public void Dispose()
        {
            if (MidiDevice != null)
                MidiDevice.Dispose();
        }
    }
}
