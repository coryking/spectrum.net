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
    public class FourPolesLowPassFilter : PropertyChangeModel, ISampleProvider
    {
        private WaveFormat waveFormat;
        private ISampleProvider source;
        
        // if true, the parameters have changed...
        private bool dirtyParams;

        private List<NAudio.Dsp.BiQuadFilter> _filters;

        public List<NAudio.Dsp.BiQuadFilter> Filters
        {
            get { return _filters; }
            private set { _filters = value; }
        }
        
        float _frequency; // peak freq


        public float Frequency
        {
            get { return _frequency; }
            set
            {
                Set(ref _frequency, value, 0.1f);
            }
        }


        private float _q;

        public float Q
        {
            get { return _q; }
            set { Set(ref _q, value, 0.1f); }
        }


        public FourPolesLowPassFilter(ISampleProvider source)
        {
            _filters = new List<NAudio.Dsp.BiQuadFilter>();
            this.source = source;
            waveFormat = source.WaveFormat;
            Frequency = 600.0f;
            Q = 0.5f;
            for (var i = 0; i < source.WaveFormat.Channels; i++)
            {
                Filters.Add(NAudio.Dsp.BiQuadFilter.LowPassFilter(source.WaveFormat.SampleRate, Frequency, Q));
                dirtyParams = false;
            }

        }

        protected override void HandlePropertyChanged(string propertyName)
        {
            dirtyParams = true;
        }
        

        public int Read(float[] buffer, int offset, int sampleCount)
        {
            //var sourceBuffer = new float[sampleCount];
            int samplesRead = source.Read(buffer, 0, sampleCount);
            if (Frequency < 1.0f || Q < 0.01f)
                return samplesRead;

            if (dirtyParams)
            {
                foreach (var filter in Filters)
                {
                    filter.SetLowPassFilter(WaveFormat.SampleRate, Frequency, Q);
                }
                dirtyParams = false;
            }
            for (var channel = 0; channel < waveFormat.Channels; channel++)
            {
                for (int i = offset + channel; i < samplesRead; i+=waveFormat.Channels)
                {
                    buffer[i] = Filters[channel].Transform(buffer[i]);

                }
            }
           
            return samplesRead;

        }

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }
    }
}
