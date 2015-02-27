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
using CorySynthUI.Models;

namespace CorySynthUI.ViewModel
{
    public class SequencerViewModel : PropertyChangeModel, IDisposable
    {
        public const int Latency = 50;
        private DeviceModel deviceModel;

        public SequencerViewModel(WaveFormat format, DeviceModel deviceModel)
        {
            VoicePanelNavItems = new ObservableCollection<string>()
            {
                "Voice",
                "Effects"
            };

            MidiChannel = new Channel(0, format);
            MidiChannel.PropertyChanged += MidiChannel_PropertyChanged;

            this.deviceModel = deviceModel;
            this.deviceModel.MidiMessageReceived +=deviceModel_MidiMessageReceived;

            this.deviceModel.SetSampleProvider(MidiChannel);

        }



        void MidiChannel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVoice")
            {
                if(MidiChannel.SelectedVoice is SamplerVoice && ((SamplerVoice)MidiChannel.SelectedVoice).Sampler is PADSynth)
                {
                    if (!VoicePanelNavItems.Contains("Oscillator"))
                        VoicePanelNavItems.Add("Oscillator");
                }
                else
                {
                    if (VoicePanelNavItems.Contains("Oscillator"))
                        VoicePanelNavItems.Remove("Oscillator");
                }
            }
        }
        
        #region Event Handlers

        void deviceModel_MidiMessageReceived(IMidiInputDevice sender, CorySignalGenerator.Sequencer.Midi.MidiInputMessageEventArgs args)
        {
            Task.Run(() =>
            {
                MidiChannel.ProcessMidiMessage(args.Message);
            });
        }

        private void MidiDevice_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            Task.Run(() =>
            {
                var msg = CorySignalGenerator.Sequencer.Midi.MidiMessageConverter.ToMidiMessage(args.Message);
                if(msg != null)
                    MidiChannel.ProcessMidiMessage(msg);
            });
        }

        #endregion

        #region Properties

        public ObservableCollection<String> VoicePanelNavItems { get; private set; }
        
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
		


        public Channel MidiChannel { get; private set; }

        #endregion

        public void Dispose()
        {
        }
    }
}
