using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
namespace CorySignalGenerator.Dsp
{
    public class DelayLine
    {
        private CircularBuffer delayLine;
        private float[] zeroBuffer;

        public DelayLine(int maxBuffer)
        {
            delayLine = new CircularBuffer(maxBuffer);
        }

        public int FromChannel { get; set; }
        public int ToChannel { get; set; }
        public int Channels { get; set; }

        public int SampleDelay { get; set; }

        public float Decay { get; set; }

        public int ConvolveDelayLine(float[] buffer, int offset, int samplesRead, float[] output, int outOffset, int count)
        {
            
            var perChannelRead = samplesRead / Channels;
            var perChannelCount = count / Channels;
            var perChannelDelay = SampleDelay / Channels;
            var totalSamplesWritten = perChannelRead; // this is the total samples written


            
            // if the amount of stuff in the buffer is more than our delay, start using it!
            if (delayLine.Count > perChannelDelay)
            {
                var channelBuffer = new float[perChannelCount];
                var maxRead = (int)Math.Min(perChannelCount, delayLine.Count);
                delayLine.Read(channelBuffer, 0, maxRead);

                // Copy the delay line into the output buffer, overwriting whatever is there.
                VectorMath.vcopy(channelBuffer, outOffset, 1, output, outOffset + ToChannel, Channels, maxRead);
                totalSamplesWritten = Math.Max(maxRead, totalSamplesWritten);
            }

            if (delayLine.Count <= perChannelDelay)
            {
                

                // We need to start writing into the buffer
                var channelBuffer = new float[perChannelCount];
                VectorMath.vscale(buffer, offset + FromChannel, Channels, channelBuffer, 0, 1, Decay, perChannelRead);
                delayLine.Write(channelBuffer, 0, perChannelRead);

            }
            // if we have come to the end of the original sample
            // we need to keep playing out the decay buffer until the end
            if (samplesRead < count)
            {
                var channelBuffer = new float[perChannelCount];
                var endOfOriginalRead = count - samplesRead;
                var leftOver = endOfOriginalRead / Channels;
                var bufferCount = (int)Math.Min(leftOver, delayLine.Count);
                delayLine.Read(channelBuffer, 0, bufferCount);
                VectorMath.vcopy(channelBuffer, 0, 1, output, outOffset + endOfOriginalRead + ToChannel, Channels, bufferCount);
                totalSamplesWritten += bufferCount;
            }
            return totalSamplesWritten;
        }


    }
}
