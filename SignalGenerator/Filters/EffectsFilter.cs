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
    public enum ReverbFilterType
    {
        GhettoReverb,
        ConvolvingReberb
    }

    public class EffectsFilter : Effect
    {
        static readonly int MaxFFTSize = 32768;

        private ISampleProvider _headProvider;
        private GhettoReverb _reverbFilter;
        private ReverbFilter _convolvingReverbFilter;
        private FourPolesLowPassFilter _lfoFilter;

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

            _lfoFilter = new FourPolesLowPassFilter(Source);

            if (WaveFormat.Channels == 2 && Source.WaveFormat.Channels != WaveFormat.Channels)
            {
                _headProvider = new MonoToStereoSampleProvider(lfoFilter);
            }
            else
            {
                _headProvider = _lfoFilter;
            }
            
            // Always instantiate the reverb filter... just don't connect it to the signal chain unless we need it
            _reverbFilter = new GhettoReverb(_headProvider);
            _convolvingReverbFilter = new ReverbFilter(_headProvider, MaxFFTSize, true);
            if (ReverbType == ReverbFilterType.GhettoReverb)
                _headProvider = _reverbFilter;
            else
                _headProvider = _convolvingReverbFilter;
            
        }

        public ReverbFilterType ReverbType
        {
            get
            {
                ReverbFilterType type;
                Enum.TryParse(ReverbFilterTypeString, out type);
                return type;
            }
        }

        public void InitConvolvingReverbFilter(WaveStream stream)
        {
            if(_convolvingReverbFilter != null)
                _convolvingReverbFilter.LoadImpuseResponseWaveStream(stream);
        }


        private string _reverbFilterTypeString = "GhettoReverb";

        /// <summary>
        /// Sets and gets the ReverbFilterType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ReverbFilterTypeString
        {
            get
            {
                return _reverbFilterTypeString;
            }
            set
            {
                Set( ref _reverbFilterTypeString, value);
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
            _lfoFilter.Frequency = LowPassCutoff;
            _lfoFilter.Q = Q;
            _reverbFilter.Decay = ReverbDecay;
            _reverbFilter.Delay = ReverbDelay;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            SetFilterValues();
            return _headProvider.Read(buffer, offset, count);
        }

    }
}
