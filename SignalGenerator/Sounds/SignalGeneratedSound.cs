using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using CorySignalGenerator.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sounds
{
    public class SignalGeneretedSound: ISoundModel
    {
        public SignalGeneretedSound(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }

        public SignalGeneratorType Type { get; set; }

        public float ReleaseSeconds { get; set; }
        public float AttackSeconds { get; set; }

        public WaveFormat WaveFormat
        {
            get;
            private set;
        }

        public ISampleProvider GetProvider(float frequency, int velocity)
        {
            return new SignalGeneratorNote(WaveFormat)
            {
                Frequency = frequency,
                Velocity = velocity,
                Type = Type,
                ReleaseSeconds = ReleaseSeconds,
                AttackSeconds = AttackSeconds
            };
        }


        public void InitSamples()
        {
            // Don't do anything.  we generate stuff on the fly
            return;
        }

    }
}
