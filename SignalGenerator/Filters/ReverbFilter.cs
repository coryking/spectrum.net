using CorySignalGenerator.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;

namespace CorySignalGenerator.Filters
{
    public class ReverbFilter : Effect
    {
        List<FFTConvolver> convolvers = new List<FFTConvolver>();

        public ReverbFilter(ISampleProvider source) : base(source)
        {

        }
        protected override void Init()
        {
            base.Init();
            
        }

        public void LoadImpuseResponseWaveStream(WaveStream stream)
        {
            if (WaveFormat.SampleRate != stream.WaveFormat.SampleRate)
                throw new InvalidOperationException("Different sample rates!");

            if (WaveFormat.Channels == stream.WaveFormat.Channels)
            {
                for (int i = 0; i < WaveFormat.Channels; i++)
                {
                    convolvers.Add(FFTConvolver.InitFromWaveStream(stream, i));
                }
            }
            else
            {
                throw new InvalidOperationException("Channel mismatch!");
            }
           
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            
            var bufferSize = (convolvers[0].FFTSize * WaveFormat.Channels);
            if (bufferSize > count)
                throw new InvalidOperationException(
                    String.Format("Not enough latency, sorry.  Need to be able to write a buffer at least {0} samples long but only have {1}", bufferSize, count));
            var fftBuffer = new float[bufferSize];
            var samplesRead = Source.Read(fftBuffer, 0, bufferSize);

            var samplesTaken = 0;

            for (int i = 0; i < WaveFormat.Channels; i++)
            {
                var channel = fftBuffer.TakeChannel(i);
                var channelOut = new float[channel.Length];
                samplesTaken += convolvers[i].Process(channel, 0, channelOut, 0,channel.Length);
                channelOut.Interleave(buffer, WaveFormat.Channels, offset + i);
                
            }

            return samplesTaken;
        }
    }
}
