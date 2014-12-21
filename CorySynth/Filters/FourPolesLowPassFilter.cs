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
    public class FourPolesLowPassFilter :ISampleProvider
    {
        private WaveFormat waveFormat;
        private ISampleProvider source;

        float omega; // peak freq
      

        public float Frequency
        {
            get { return omega; }
            set
            {
                omega = value;
            }
        }


        public FourPolesLowPassFilter(ISampleProvider source)
        {
            this.source=source;
            waveFormat = source.WaveFormat;

            Frequency = 600.0f;

        }

        float in_1 = 0;
        float in_2 = 0;
        float out_1 = 0;
        float out_2 = 0;
            

        public int Read(float[] buffer, int offset, int sampleCount)
        {

            int samplesRead = source.Read(buffer, offset, sampleCount);
            float r=1;
            float c = 1.0f / (float)Math.Tan(Math.PI * Frequency / waveFormat.SampleRate) * 0.957f;
		      float a1 = 1.0f / ( 1.0f + r * c + c * c);
		      float a2 = 2* a1;
		      float a3 = a1;
		      float b1 = 2.0f * ( 1.0f - c*c) * a1;
		      float b2 = ( 1.0f - r * c + c * c) * a1;

            
            var queue = new Queue<float>();
            queue.Enqueue(buffer[0+offset]);
            queue.Enqueue(buffer[1 + offset]);
            queue.Enqueue(buffer[2 + offset]);

		      buffer[0+offset]= a1 * queue.ElementAt(0) + a2 * in_1 + a3 * in_2 - b1*out_1 - b2*out_2;
		      buffer[1+offset]= a1 * queue.ElementAt(1) + a2 * queue.ElementAt(0) + a3 * in_1 - b1*buffer[0+offset] - b2*out_1;
		      queue.Dequeue();
		      //double max = 0;
		      //System.out.println("in-1" + in_1 + "in-2" + in_2 + "out-1" + out_1 + "out-2" + out_2); 
		      for (int i = 2; i < samplesRead; i++) {
                  queue.Enqueue(buffer[i+offset]);
		    	  buffer[i+offset] = a1 * queue.ElementAt(0) + a2 * queue.ElementAt(1) + a3 * queue.ElementAt(2) - b1*buffer[i-1 + offset] - b2*buffer[i-2 + offset];
		    	  //if(out[i]< max)max = out[i];
		        queue.Dequeue();
              }
		      //System.out.println("sampleRate = "+ sampleRate +  "f = "+ freq + "  Coefs: a1 " + a1  +" a2"+ a2 +" a3"+ a3 +" b1 "+ b1 +" b2 " +b2);
		      //System.out.println("out-1 = " + out[count-1]);
		      //System.out.println("out-2 = " + out[count-2]);
		      //System.out.println("MAX = " + max);
		      
		      in_1 = queue.Dequeue();
		      in_2 = queue.Dequeue();
		      out_1 = buffer[samplesRead-1 + offset];
		      out_2 = buffer[samplesRead-2 + offset];
            
            return samplesRead;

        }

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }
    }
}
