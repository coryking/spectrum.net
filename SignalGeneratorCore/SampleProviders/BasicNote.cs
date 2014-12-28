using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGeneratorCore.SampleProviders
{
    public class BasicNote : ISampleProvider
    {
        public int Velocity { get; set; }
        public SignalGeneratorType Type { get; set; }
        public float Frequency { get; set; }

        public float ReleaseSeconds { get; set; }
        public float AttackSeconds { get; set; }

        private AdsrSampleProvider _provider;

        public BasicNote(int sampleRate, int channels)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        private void RebuildChain()
        {
            var wave = new SignalGenerator(WaveFormat.SampleRate, WaveFormat.Channels)
                {
                    Frequency = Frequency,
                    Type = Type
                };
            var volume = new VolumeSampleProvider(wave)
            {
                Volume = Velocity / 128.0f
            };
            _provider = new AdsrSampleProvider(volume)
            {
                ReleaseSeconds = ReleaseSeconds,
                AttackSeconds = AttackSeconds
            };
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (_provider == null)
                RebuildChain();
            return _provider.Read(buffer, offset, count);
        }

        public void StopNote()
        {
            if(_provider != null)
                _provider.Stop();
        }

        public WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
