using CorySignalGenerator.Filters;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    public abstract class AdsrNote : ISampleProvider, IStoppableSample
    {
        public int Velocity { get; set; }


        public float ReleaseSeconds { get; set; }
        public float AttackSeconds { get; set; }


        private AdsrSampleProvider _provider;

        public AdsrNote(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }

        protected abstract ISampleProvider GetSampler();
       
        protected void RebuildChain()
        {
            var wave = GetSampler();
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

        public void Stop()
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
