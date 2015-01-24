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

        public SamplerVoice(ISampler sampler)
        {
            Sampler = sampler;
            WaveFormat = sampler.WaveFormat;
            Controller = new VoiceController(sampler);
            Effects = new SignalChain<IEffect>(Controller);
        }

        protected VoiceController Controller { get; set; }

        /// <summary>
        /// The sampler for this voice
        /// </summary>
        public ISampler Sampler { get; private set; }

        /// <summary>
        /// List of effects to apply to this voice (note that these are not factories, so they have to handle having their values changed while running)
        /// </summary>
        public SignalChain<IEffect> Effects { get; private set; }

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
            return Effects.Read(buffer, offset, count);
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
