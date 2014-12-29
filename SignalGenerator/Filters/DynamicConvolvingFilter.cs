using NAudio.Wave;
using CorySignalGenerator.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class DynamicConvolvingFilter :ISampleProvider
    {
        private List<DynamicConvolver> convolvers;
        private ISampleProvider source;
        public DynamicConvolvingFilter(ISampleProvider source, bool normalize=true)
        {
            if (normalize)
                this.source = new NormalizingFilter(source);
            else
                this.source = source;
            convolvers = new List<DynamicConvolver>();
            for (var channel = 0; channel < source.WaveFormat.Channels; channel++)
            {
                convolvers.Add(new DynamicConvolver(source.WaveFormat.SampleRate, 2, 1000));
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = source.Read(buffer, offset, count);
            for (var i = 0; i < samplesRead; i++)
            {
                var channel = i % source.WaveFormat.Channels;
                buffer[i+offset] = convolvers[channel].Operator(buffer[i+offset]);
            }
            return samplesRead;
        }

        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }


    }
}
