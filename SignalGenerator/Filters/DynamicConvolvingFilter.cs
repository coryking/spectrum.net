using NAudio.Wave;
using CorySignalGenerator.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class DynamicConvolvingFilter :Effect
    {
        private List<DynamicConvolver> convolvers;
        private ISampleProvider _head;
        private bool normalizeSample;
        public DynamicConvolvingFilter(ISampleProvider source, bool normalize=true) : base(source)
        {
            normalizeSample = normalize;
        }
        protected override void Init()
        {
            base.Init();
            if (normalizeSample)
                _head = new NormalizingFilter(Source);
            else
                _head = Source;

            convolvers = new List<DynamicConvolver>();
            for (var channel = 0; channel < WaveFormat.Channels; channel++)
            {
                convolvers.Add(new DynamicConvolver(SampleRate, 2, 1000));
            }
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _head.Read(buffer, offset, count);
            for (var i = 0; i < samplesRead; i++)
            {
                var channel = i % Source.WaveFormat.Channels;
                buffer[i+offset] = convolvers[channel].Operator(buffer[i+offset]);
            }
            return samplesRead;
        }

       

    }
}
