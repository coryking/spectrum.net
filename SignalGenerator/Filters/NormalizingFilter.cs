using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    /// <summary>
    /// This filter does its best to make sure the sample buffer values never go above zero
    /// </summary>
    public class NormalizingFilter : Effect
    {
        float maxValue = 0;
        public NormalizingFilter(ISampleProvider source) : base(source)
        {
        }
        protected override int OnRead(float[] buffer, int offset, int count)
        {
            var samplesRead = Source.Read(buffer, offset, count);
            for (int n = 0; n < samplesRead; n++)
            {
                maxValue = Math.Max(maxValue, Math.Abs(buffer[n + offset]));
            }
            if(maxValue > 1.0)
            {
                for (int n = 0; n < samplesRead; n++)
                    buffer[n + offset] /= maxValue;
            }
            return samplesRead;
        }


        public override string Name { get { return "Normalizing Filter"; } }
    }
}
