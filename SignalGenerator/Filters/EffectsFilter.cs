using CorySignalGenerator.Filters;
using CorySignalGenerator.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CorySignalGenerator.Filters
{
    public enum ReverbFilterType
    {
        None,
        GhettoReverb,
        ConvolvingReberb
    }

    public class EffectsFilter : Effect
    {
        static readonly int MaxFFTSize = 32768;

        private ISampleProvider _headProvider;

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
        }


        private void RebuildSignalChain()
        {

            _lfoFilter = new FourPolesLowPassFilter(Source);

            if (WaveFormat.Channels == 2 && Source.WaveFormat.Channels != WaveFormat.Channels)
            {
                _headProvider = new MonoToStereoSampleProvider(_lfoFilter);
            }
            else
            {
                _headProvider = _lfoFilter;
            }
            
            // Always instantiate the reverb filter... just don't connect it to the signal chain unless we need it
            ChorusEffectFilter = new ChorusEffect(_headProvider);


            GhettoReverbFilter = new GhettoReverb(ChorusEffectFilter);
            _headProvider = GhettoReverbFilter;
            Debug.WriteLine("Head Provider is: {0}", _headProvider);
            
        }

        public void InitConvolvingReverbFilter(WaveStream stream)
        {
            if(_convolvingReverbFilter != null)
                _convolvingReverbFilter.LoadImpuseResponseWaveStream(stream);
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

        private float _q = 0.5f;
        public float Q
        {
            get { return _q; }
            set
            {
                Set(ref _q, value);
            }
        }

        /// <summary>
        /// Is the reverb effect ready?
        /// 
        /// TODO: Make this return false when using the Convolving reverb filters
        /// </summary>
        public bool IsReverbReady
        {
            get
            {
                return true;
            }
        }


        ChorusEffect _chorusEffect;

        public ChorusEffect ChorusEffectFilter
        {
            get { return _chorusEffect; }
            set { Set(ref _chorusEffect, value); }
        }
       
        private GhettoReverb _reverbFilter;

        public GhettoReverb GhettoReverbFilter
        {
            get { return _reverbFilter; }
            set { Set(ref _reverbFilter, value); }
        }

        protected void SetFilterValues()
        {
            _lfoFilter.LPFrequency = LowPassCutoff;
            _lfoFilter.Q = Q;
            // GhettoReverbFilter.Decay = ReverbDecay;
            //GhettoReverbFilter.Delay = ReverbDelay;
        }

        protected override int OnRead(float[] buffer, int offset, int count)
        {
            SetFilterValues();
            return _headProvider.Read(buffer, offset, count);
        }

    }
}
