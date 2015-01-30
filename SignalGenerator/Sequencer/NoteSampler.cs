using CorySignalGenerator.Models;
using CorySignalGenerator.SampleProviders;
using CorySignalGenerator.Sequencer.Interfaces;
using CorySignalGenerator.Utils;
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
    public abstract class NoteSampler : PropertyChangeModel, ISampler
    {
        public NoteSampler()
        {
            Effects = new ObservableCollection<IEffectFactory>();
        }

        public abstract NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            protected set;
        }

        public INote GetNote(Models.MidiNote note, float velocity)
        {
            // Make sure we are totally cool...
            Debug.Assert(Envelope != null);
            if (Envelope == null)
                return null;

            var frequency = FrequencyUtils.ScaleFrequency((float)note.Frequency, Pitch, 12f);
            note.Frequency = frequency;

            ISampleProvider lastSampler = GenerateNote(note);
            
            Debug.Assert(lastSampler != null);
            if (lastSampler == null)
                return null;
            
            // wrap the sampler in its effects...
            foreach (var effect in Effects)
            {
                lastSampler = effect.GetEffect(lastSampler);
            }

            if (SupportsVelocity)
                lastSampler = new VolumeSampleProvider(lastSampler) { Volume = velocity };

            Debug.Assert(Envelope != null);
            if (Envelope == null)
                return null;

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


        #region Property Pitch
        private float _pitch = 0f;

        /// <summary>
        /// Sets and gets the Pitch property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                Set(ref _pitch, value,-12f,12f);
            }
        }
        #endregion
		

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
