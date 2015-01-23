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
            
            Effects = new SignalChain<IEffect>(Mixer);
            
            Controller = new ChannelController(Voices, Effects, ChannelNumber);
        }

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
        public SignalChain<IEffect> Effects { get; private set; }

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
