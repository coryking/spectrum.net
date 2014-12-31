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
    public class EffectsFilter : PropertyChangeModel, ISampleProvider
    {
        private ISampleProvider _source;
        private ISampleProvider _headProvider;
        
        public EffectsFilter(ISampleProvider source, int outputChannels)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, outputChannels);
            _source = source;
            RebuildSignalChain();
            LowPassCutoff = 22000.0f;
            Q = 0.5f;
            ReverbDelay = 100;
            ReverbDecay = 0.2f;
           

        }

        private FourPolesLowPassFilter lfoFilter;
        private GhettoReverb reverbFilter;

        private void RebuildSignalChain()
        {

            lfoFilter = new FourPolesLowPassFilter(_source);
            if (WaveFormat.Channels == 2 && _source.WaveFormat.Channels != WaveFormat.Channels)
            {
                _headProvider = new MonoToStereoSampleProvider(lfoFilter);
            }
            else
            {
                _headProvider = lfoFilter;
            }
            reverbFilter = new GhettoReverb(_headProvider);
            _headProvider = reverbFilter; 
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
            reverbFilter.Decay = ReverbDecay;
            reverbFilter.Delay = ReverbDelay;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            SetFilterValues();
            return _headProvider.Read(buffer, offset, count);
        }

        public WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
