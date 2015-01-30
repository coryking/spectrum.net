using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NAudio.Wave.SampleProviders;
using MoreLinq;
using System.Diagnostics;
using CorySignalGenerator.Models;

namespace CorySignalGenerator.Sequencer
{
    public class Channel : IChannel
    {
        private object _lock = new object();
        public Channel(int channelNumber, WaveFormat format)
        {
            WaveFormat = format;
            ChannelNumber = channelNumber;

            Voices = new ObservableCollection<IVoice>();
            Voices.CollectionChanged += Voices_CollectionChanged;
            
            Mixer = new MixingSampleProvider(format);
            Mixer.ReadFully = true;
            
            Effects = new EffectChain(Mixer);
            
            Controller = new ChannelController(Voices, Effects, ChannelNumber);

            AddVoiceCommand = new RelayCommand(OnAddVoiceCommand);
            RemoveVoiceCommand = new RelayCommand(OnRemoveVoiceCommand);
        }

        #region Relay Commands

        public RelayCommand AddVoiceCommand { get; private set; }

        private void OnAddVoiceCommand(object parameter)
        {
            var samplerType = parameter as Type;
            if (samplerType == null)
                return;

            var sampler = Activator.CreateInstance(samplerType, WaveFormat) as ISampler;
            if (sampler == null)
                return;

            this.Voices.Add(new SamplerVoice(sampler));
        }

        public RelayCommand RemoveVoiceCommand { get; private set; }

        private void OnRemoveVoiceCommand(object parameter)
        {
            var voice = parameter as IVoice;
            if (voice == null)
                return;

            this.Voices.Remove(voice);
        }

        #endregion

        #region Event Handlers

        void Voices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (_lock)
            {
                Mixer.RemoveAllMixerInputs();
                Voices.ForEach(AddMixerInput);
            }
        }

        private void AddMixerInput(ISampleProvider provider)
        {
            // Make sure all the inputs have the same waveformat...
            Debug.Assert(provider.WaveFormat == WaveFormat);
            if (provider.WaveFormat != WaveFormat)
                return;
            Mixer.AddMixerInput(provider);
        }

        #endregion

        #region Properties

        public int ChannelNumber
        {
            get;
            private set;
        }

        public ObservableCollection<IVoice> Voices { get; private set; }
        public EffectChain Effects { get; private set; }

        protected ChannelController Controller { get; private set; }

        protected MixingSampleProvider Mixer {get; private set;}


        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            private set;
        }

        #endregion

        public void ProcessMidiMessage(Midi.IMidiMessage message)
        {
            Controller.ProcessMidiMessage(message);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                return Effects.Read(buffer, offset, count);
            }
        }

    }
}
