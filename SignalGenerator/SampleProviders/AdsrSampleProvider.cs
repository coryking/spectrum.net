using CorySignalGenerator.Filters;
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
    public class AdsrSampleProvider : Effect, IStoppableSample
    {
        private EnvelopeGenerator adsr;
        private float attackSeconds;
        private float releaseSeconds;

        /// <summary>
        /// Creates a new AdsrSampleProvider with default values
        /// </summary>
        public AdsrSampleProvider(ISampleProvider source) : base(source)
        {
            
        }
        protected override void Init()
        {
            base.Init();
            adsr = new EnvelopeGenerator();
            AttackSeconds = 0.01f;
            adsr.SustainLevel = 1.0f;
            adsr.DecayRate = 0.0f * SampleRate;
            ReleaseSeconds = 0.3f;
            adsr.Gate(true);

        }

        /// <summary>
        /// Attack time in seconds
        /// </summary>
        public float AttackSeconds
        {
            get
            {
                return attackSeconds;
            }
            set
            {
                attackSeconds = value;
                adsr.AttackRate = attackSeconds * SampleRate;
            }
        }

        /// <summary>
        /// Release time in seconds
        /// </summary>
        public float ReleaseSeconds
        {
            get
            {
                return releaseSeconds;
            }
            set
            {
                releaseSeconds = value;
                adsr.ReleaseRate = releaseSeconds * SampleRate;
            }
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        public override int Read(float[] buffer, int offset, int count)
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
            adsr.Gate(false);
        }

    }
}
