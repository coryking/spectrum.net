using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySynth.Filters
{
    /// <summary>
    /// A "Four Poles" low pass filter
    /// </summary>
    /// <remarks>See http://musicalagents.googlecode.com/svn/trunk/ensemble/src/ensemble/audio/dsp/FilterProcessing.java </remarks>
    public class FourPolesLowPassFilter : ISampleProvider
    {
        private WaveFormat waveFormat;
        private ISampleProvider source;

        private List<NAudio.Dsp.BiQuadFilter> filter;
        
        float _frequency; // peak freq


        public float Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                SetParams();
            }
        }


        private float _q;

        public float Q
        {
            get { return _q; }
            set { _q = value; SetParams(); }
        }


        public FourPolesLowPassFilter(ISampleProvider source)
        {
            filter = new List<NAudio.Dsp.BiQuadFilter>();
            this.source = source;
            waveFormat = source.WaveFormat;
            Frequency = 600.0f;
            Q = 0.5f;

        }

        /// <summary>
        ///  rename
        /// </summary>

        private void SetParams()
        {
            filter.Clear();
            for (var i = 0; i < WaveFormat.Channels; i++)
                filter.Add(NAudio.Dsp.BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, Frequency, Q));
        }
       

        public int Read(float[] buffer, int offset, int sampleCount)
        {
            //var sourceBuffer = new float[sampleCount];
            int samplesRead = source.Read(buffer, 0, sampleCount);

            for (var channel = 0; channel < waveFormat.Channels; channel++)
            {
                for (int i = offset + channel; i < samplesRead; i+=waveFormat.Channels)
                {
                    buffer[i] = filter[channel].Transform(buffer[i]);

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
