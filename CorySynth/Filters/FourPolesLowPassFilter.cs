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

        private NAudio.Dsp.BiQuadFilter filter;
        
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
            filter = NAudio.Dsp.BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, Frequency, Q);
        }
       

        public int Read(float[] buffer, int offset, int sampleCount)
        {
            //var sourceBuffer = new float[sampleCount];
            int samplesRead = source.Read(buffer, 0, sampleCount);


            //inQueue.Enqueue(sourceBuffer[0]);
            //inQueue.Enqueue(sourceBuffer[1]);
            //inQueue.Enqueue(sourceBuffer[2]);

            //buffer[0 + offset] = a1 * inQueue.ElementAt(0) + a2 * in_1 + a3 * in_2 - b1 * out_1 - b2 * out_2;
            //buffer[1 + offset] = a1 * inQueue.ElementAt(1) + a2 * inQueue.ElementAt(0) + a3 * in_1 - b1 * buffer[0 + offset] - b2 * out_1;
            //inQueue.Dequeue();
            //double max = 0;
            //System.out.println("in-1" + in_1 + "in-2" + in_2 + "out-1" + out_1 + "out-2" + out_2); 
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i + offset] = filter.Transform(buffer[i + offset]); //Process(buffer[i+offset]);

            }
            //System.out.println("sampleRate = "+ sampleRate +  "f = "+ freq + "  Coefs: a1 " + a1  +" a2"+ a2 +" a3"+ a3 +" b1 "+ b1 +" b2 " +b2);
            //System.out.println("out-1 = " + out[count-1]);
            //System.out.println("out-2 = " + out[count-2]);
            //System.out.println("MAX = " + max);

            //in_1 = inQueue.Dequeue();
            //in_2 = inQueue.Dequeue();
            //out_1 = buffer[samplesRead - 1 + offset];
            //out_2 = buffer[samplesRead - 2 + offset];

            return samplesRead;

        }

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }
    }
}
