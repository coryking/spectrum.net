﻿using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    /// <summary>
    /// ADSR sample provider allowing you to specify attack, decay, sustain and release values
    /// </summary>
    public class AdsrSampleProvider : ISampleProvider, IStoppableSample
    {
        private readonly ISampleProvider source;
        private readonly EnvelopeGenerator adsr;
        private float attackSeconds;
        private float releaseSeconds;

        /// <summary>
        /// Creates a new AdsrSampleProvider with default values
        /// </summary>
        public AdsrSampleProvider(ISampleProvider source)
        {
            //if (source.WaveFormat.Channels > 1) throw new ArgumentException("Currently only supports mono inputs");
            this.source = source;
            adsr = new EnvelopeGenerator();
            AttackSeconds = 0.01f;
            adsr.SustainLevel = 1.0f;
            adsr.DecayRate = 0.0f * WaveFormat.SampleRate;
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
                adsr.AttackRate = attackSeconds * WaveFormat.SampleRate;
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
                adsr.ReleaseRate = releaseSeconds * WaveFormat.SampleRate;
            }
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            if (adsr.State == EnvelopeGenerator.EnvelopeState.Idle) return 0; // we've finished
            var samples = source.Read(buffer, offset, count);
            for (int n = 0; n < samples/WaveFormat.Channels; n++)
            {
                for (int x = 0; x < WaveFormat.Channels;x++ )
                {
                    var multiplier = adsr.Process();
                    buffer[offset++] *= multiplier;
                }
            }
            return samples;
        }

        /// <summary>
        /// Enters the Release phase
        /// </summary>
        public void Stop()
        {
            adsr.Gate(false);
        }

        /// <summary>
        /// The output WaveFormat
        /// </summary>
        public WaveFormat WaveFormat { get { return source.WaveFormat; } }
    }
}
