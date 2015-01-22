using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer
{
    public abstract class NoteSampler :ISampler
    {
        public NoteSampler()
        {
            Effects = new ObservableCollection<IEffectFactory>();
        }

        public abstract NAudio.Wave.WaveFormat WaveFormat
        {
            get;
        }

        public INote GetNote(Models.MidiNote note, float velocity)
        {
            // Make sure we are totally cool...
            Debug.Assert(Envelope != null);
            if (Envelope == null)
                return null;

            ISampleProvider lastSampler = GenerateNote(note);
            // wrap the sampler in its effects...
            foreach (var effect in Effects)
            {
                lastSampler = effect.GetEffect(lastSampler);
            }

            if (SupportsVelocity)
                lastSampler = new VolumeSampleProvider(lastSampler);

            IEnvelopeEffect envelope = Envelope.GetEnvelope(lastSampler);

            // For now, all notes are sustainable but in the future if we have a drum
            // kit or something we can return a different kind of noe
            return new SustainableNote(envelope);
        }

        /// <summary>
        /// The real meat of this whole deal... the thing that generates the actual sample...
        /// </summary>
        /// <param name="note">The note to generate</param>
        /// <returns>Give me a sample provider!</returns>
        protected abstract ISampleProvider GenerateNote(Models.MidiNote note);

        /// <summary>
        /// If true, will make sure to apply a velocity sampler
        /// </summary>
        protected abstract bool SupportsVelocity { get; }

        public abstract string Name
        {
            get;
        }

        public ObservableCollection<IEffectFactory> Effects
        {
            get;
            private set;
        }

        public IEnvelopeEffectFactory Envelope
        {
            get;
            set;
        }
    }
}
