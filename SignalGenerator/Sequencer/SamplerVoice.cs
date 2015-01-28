using CorySignalGenerator.Models;
using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
    public class SamplerVoice : PropertyChangeModel, IVoice
    {
        private VolumeSampleProvider _volumeSampler;


        public SamplerVoice(ISampler sampler)
        {
            Sampler = sampler;
            WaveFormat = sampler.WaveFormat;
            Controller = new VoiceController(sampler);
            Effects = new EffectChain(Controller);
            _volumeSampler = new VolumeSampleProvider(Effects);
        }

        protected VoiceController Controller { get; set; }

        /// <summary>
        /// The sampler for this voice
        /// </summary>
        public ISampler Sampler { get; private set; }


        #region Property Volume
        /// <summary>
        /// Gets/Sets the volume for this control.
        /// </summary>
        public float Volume
        {
            get
            {
                return _volumeSampler.Volume;
            }
            set
            {
                float volume = 0;
                Set(ref volume, value, 0.0f, 1.0f);
                _volumeSampler.Volume = volume;
            }
        }
        #endregion
		

        /// <summary>
        /// List of effects to apply to this voice (note that these are not factories, so they have to handle having their values changed while running)
        /// </summary>
        public EffectChain Effects { get; private set; }

        public string Name
        {
            get { return Sampler.Name; }
        }

        public void NoteOn(Models.MidiNote note, float velocity)
        {
            Controller.NoteOn(note, velocity);
        }

        public void NoteOff(Models.MidiNote note)
        {
            Controller.NoteOff(note);
        }

        public void SustainOn()
        {
            Controller.SustainOn();
        }

        public void SustainOff()
        {
            Controller.SustainOff();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _volumeSampler.Read(buffer, offset, count);
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
