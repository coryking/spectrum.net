using CorySignalGenerator.Filters;
using CorySignalGenerator.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CorySignalGenerator.Filters
{
    public class EffectsFilter : Effect
    {
        private ISampleProvider _headProvider;

        public EffectsFilter(ISampleProvider source, int outputChannels) : base(source)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, outputChannels);
        }
        protected override void Init()
        {
            RebuildSignalChain();
            LowPassCutoff = 22000.0f;
            Q = 0.5f;
            ReverbDelay = 100;
            ReverbDecay = 0.2f;
        }

        private FourPolesLowPassFilter lfoFilter;

        private void RebuildSignalChain()
        {

            lfoFilter = new FourPolesLowPassFilter(Source);
            if (WaveFormat.Channels == 2 && Source.WaveFormat.Channels != WaveFormat.Channels)
            {
                _headProvider = new MonoToStereoSampleProvider(lfoFilter);
            }
            else
            {
                _headProvider = lfoFilter;
            }
        }


        private float _reverbDelay;
        public float ReverbDelay
        {
            get { return _reverbDelay; }
            set
            {
                Set(ref _reverbDelay, value, 0.0f, 2000.0f);
            }
        }

        private float _reverbDecay;
        public float ReverbDecay
        {
            get { return _reverbDecay; }
            set {
                Set(ref _reverbDecay, value, 0.0f, 0.49f);
            }
        }

        private float _lfoFrequency;
        public float LowPassCutoff
        {
            get { return _lfoFrequency; }
            set
            {
                Set(ref _lfoFrequency, value);

            }
        }

        private float _q;
        public float Q
        {
            get { return _q; }
            set
            {
                Set(ref _q, value);
            }
        }

        protected void SetFilterValues()
        {
            lfoFilter.Frequency = LowPassCutoff;
            lfoFilter.Q = Q;
            //reverbFilter.Decay = ReverbDecay;
            //reverbFilter.Delay = ReverbDelay;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            SetFilterValues();
            return _headProvider.Read(buffer, offset, count);
        }

    }
}
