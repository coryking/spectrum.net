using CorySignalGenerator.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    /// <summary>
    /// A "Four Poles" low pass filter
    /// </summary>
    /// <remarks>See http://musicalagents.googlecode.com/svn/trunk/ensemble/src/ensemble/audio/dsp/FilterProcessing.java </remarks>
    public class FourPolesLowPassFilter : Effect
    {
        
        // if true, the parameters have changed...
        private bool dirtyParams;

        protected List<NAudio.Dsp.BiQuadFilter> LPFilters
        {
            get;
            private set;
        }

        protected List<NAudio.Dsp.BiQuadFilter> HPFilters
        {
            get;
            private set;
        }

        public override string Name { get { return "Bandpass Filter"; } }


        #region Property HPFrequency
        private float _hpfrequency = 1f;

        /// <summary>
        /// Sets and gets the HPFrequency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float HPFrequency
        {
            get
            {
                return _hpfrequency;
            }
            set
            {
                Set(ref _hpfrequency, value);
            }
        }
        #endregion
		

        float _lpfrequency=22000f; // peak freq


        public float LPFrequency
        {
            get { return _lpfrequency; }
            set
            {
                Set(ref _lpfrequency, value, 0.1f);
            }
        }


        private float _q=0.5f;

        public float Q
        {
            get { return _q; }
            set { Set(ref _q, value, 0.1f); }
        }


        public FourPolesLowPassFilter(ISampleProvider source) :base(source)
        {
            

        }
        protected override void Init()
        {
            LPFilters = new List<NAudio.Dsp.BiQuadFilter>();
            for (var i = 0; i < WaveFormat.Channels; i++)
            {
                LPFilters.Add(NAudio.Dsp.BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, LPFrequency, Q));
                dirtyParams = false;
            }
            HPFilters = new List<NAudio.Dsp.BiQuadFilter>();
            for (var i = 0; i < WaveFormat.Channels; i++)
            {
                HPFilters.Add(NAudio.Dsp.BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, HPFrequency, Q));
                dirtyParams = false;
            }
        }

        protected override void HandlePropertyChanged(string propertyName)
        {
            dirtyParams = true;
        }
        
        protected override int OnRead(float[] buffer, int offset, int sampleCount)
        {
            //var sourceBuffer = new float[sampleCount];
            int samplesRead = Source.Read(buffer, 0, sampleCount);
            if (LPFrequency < 1.0f || Q < 0.01f)
                return samplesRead;

            if (dirtyParams)
            {
                
                foreach (var filter in LPFilters)
                {
                    filter.SetLowPassFilter(SampleRate, LPFrequency, Q);
                }
                foreach (var filter in HPFilters)
                {
                    filter.SetHighPassFilter(SampleRate, HPFrequency, Q);
                }

                dirtyParams = false;
            }
            for (var channel = 0; channel < WaveFormat.Channels; channel++)
            {
                if (LPFrequency > 0)
                    for (int i = offset + channel; i < samplesRead; i += WaveFormat.Channels)
                        buffer[i] = LPFilters[channel].Transform(buffer[i]);

                if (HPFrequency > 0)
                    for (int i = offset + channel; i < samplesRead; i+=WaveFormat.Channels )
                        buffer[i] = HPFilters[channel].Transform(buffer[i]);


                
            }
           
            return samplesRead;

        }

    }
}
