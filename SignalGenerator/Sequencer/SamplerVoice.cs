using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer
{
    /// <summary>
    /// A voice that has a sampler and a bunch of effects
    /// </summary>
    public class SamplerVoice : IVoice
    {
        VoiceController _controller;
        private bool effectsDirty;
        private object _lock = new object();

        public SamplerVoice(ISampler sampler)
        {
            Sampler = sampler;
            WaveFormat = sampler.WaveFormat;
            _controller = new VoiceController(sampler);
            Effects = new ObservableCollection<IEffect>();
            Effects.CollectionChanged += Effects_CollectionChanged;
            effectsDirty = true;
        }

        void Effects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (_lock)
            {
                effectsDirty = true;
            }
        }
        protected VoiceController Controller { get; set; }

        /// <summary>
        /// The sampler for this voice
        /// </summary>
        public ISampler Sampler { get; private set; }

        /// <summary>
        /// List of effects to apply to this voice (note that these are not factories, so they have to handle having their values changed while running)
        /// </summary>
        public ObservableCollection<IEffect> Effects { get; private set; }

        public string Name
        {
            get;
            protected set;
        }

        public void NoteOn(Models.MidiNote note, float velocity)
        {
            _controller.NoteOn(note, velocity);
        }

        public void NoteOff(Models.MidiNote note)
        {
            _controller.NoteOff(note);
        }

        public void SustainOn()
        {
            _controller.SustainOn();
        }

        public void SustainOff()
        {
            _controller.SustainOff();
        }

        protected ISampleProvider EffectsChain { get; set; }
        protected void RebuildEffectsChain()
        {
            lock (_lock)
            {
                ISampleProvider lastSource = Controller;
                foreach (var effect in Effects)
                {
                    // Sanity check... everything must have the same wave format.
                    if (effect.WaveFormat != lastSource.WaveFormat)
                        throw new InvalidOperationException(String.Format("Sample providers do not share same WaveFormat {0}, {1}", lastSource, effect));

                    effect.Source = lastSource;
                    lastSource = effect;
                }
                EffectsChain = lastSource;
                effectsDirty = false;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (effectsDirty)
                RebuildEffectsChain();

            return EffectsChain.Read(buffer, offset, count);

        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
