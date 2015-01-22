using CorySignalGenerator.Filters;
using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    /// <summary>
    /// ADSR sample provider allowing you to specify attack, decay, sustain and release values
    /// </summary>
    public class AdsrSampleProvider : Effect, IEnvelopeEffect, IStoppableSample, ISustainable
    {
        protected bool NoteStopped;
        protected bool SustainActive;

        private EnvelopeGenerator adsr;


        /// <summary>
        /// Creates a new AdsrSampleProvider with default values
        /// </summary>
        public AdsrSampleProvider(ISampleProvider source, float attackMs, float decayMs, float sustainLevel, float releaseMs) : base(source)
        {
            AttackMs = attackMs;
            ReleaseMs = releaseMs;
            DecayMs = decayMs;
            SustainLevel = sustainLevel;
            adsr = new EnvelopeGenerator();
            SetAdsrValues();
            adsr.Gate(true);
        }

        public override string Name { get { return "Adsr Sample Provider"; } }

        /// <summary>
        /// Attack time in ms
        /// </summary>
        public float AttackMs { get; private set; }
        /// <summary>
        /// Release time in ms
        /// </summary>
        public float ReleaseMs { get; private set; }

        /// <summary>
        /// Decay time in ms
        /// </summary>
        public float DecayMs { get; private set; }

        public float SustainLevel { get; private set; }

        protected void SetAdsrValues()
        {
            adsr.ReleaseRate = FromMsToSampleRate(ReleaseMs);
            adsr.AttackRate = FromMsToSampleRate(AttackMs);
            adsr.DecayRate = FromMsToSampleRate(DecayMs);
            adsr.SustainLevel = SustainLevel;
        }

        /// <summary>
        /// Convert milliseconds to samples
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected float FromMsToSampleRate(float value)
        {
            return value * SampleRate / 1000;
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        protected override int OnRead(float[] buffer, int offset, int count)
        {
            if (adsr.State == EnvelopeGenerator.EnvelopeState.Idle) return 0; // we've finished
            var samplesTaken = Source.Read(buffer, offset, count);

            float multiplier = 0;
            var samplesWritten = 0;
            while (samplesWritten < samplesTaken && adsr.State != EnvelopeGenerator.EnvelopeState.Idle)
            {

                if (samplesWritten % Channels == 0)
                    multiplier = adsr.Process();

                buffer[offset++] *= multiplier;
                samplesWritten++;
            }
            return samplesWritten;
        }

        /// <summary>
        /// Enters the Release phase
        /// </summary>
        public void Stop()
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
                adsr.Gate(false);

            }

        }

    }
}
