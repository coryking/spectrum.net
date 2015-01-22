using CorySignalGenerator.Sequencer.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer
{
    public sealed class SustainableNote :INote
    {
        protected bool NoteStopped;
        protected bool SustainActive;
        IEnvelopeEffect _envelopeSampler;

        public SustainableNote(IEnvelopeEffect envelopeSampler)
        {
            _envelopeSampler = envelopeSampler;
        }

        /// <summary>
        /// Enters the Release phase
        /// </summary>
        public void NoteOff()
        {
            Debug.WriteLine("Stop Note Triggered");
            NoteStopped = true;
            HandleStopping();
        }


        public void SustainOn()
        {
            Debug.WriteLine("Sustain Note Triggered");
            if (!NoteStopped)
            {
                SustainActive = true;
            }
        }

        public void SustainOff()
        {
            Debug.WriteLine("End Sustain Triggered");
            SustainActive = false;
            HandleStopping();
        }

        private void HandleStopping()
        {
            // Only stop the note if the pedal isn't down...
            // Otherwise, stop the note when the pedal is lifted.
            if (!SustainActive && NoteStopped)
            {
                Debug.WriteLine("Going to stop note");
                _envelopeSampler.Stop();

            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _envelopeSampler.Read(buffer, offset, count);
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get { return _envelopeSampler.WaveFormat; }
        }
    }
}
