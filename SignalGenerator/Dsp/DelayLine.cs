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
        public int ConvolveDelayLine(float[] buffer, int offset, float[] output, int outOffset, int count, int samplesRead, float decay, int sampleDelay, int fromChannel, int toChannel, int totalChannels)
        {
            
            var perChannelRead = samplesRead / totalChannels;
            var perChannelCount = count / totalChannels;
            var perChannelDelay = sampleDelay / totalChannels;
            var totalSamplesWritten = perChannelRead; // this is the total samples written
            // if the amount of stuff in the buffer is more than our delay, start using it!
            if (delayLine.Count > perChannelDelay)
            {
                var maxRead = (int)Math.Min(perChannelCount, delayLine.Count);
                var channelBuffer = new float[perChannelCount];
                delayLine.Read(channelBuffer, 0, maxRead);

                // Copy the delay line into the output buffer, overwriting whatever is there.
                VectorMath.vcopy(channelBuffer, outOffset, 1, output, outOffset + toChannel, totalChannels, maxRead);
                totalSamplesWritten = Math.Max(maxRead, totalSamplesWritten);
            }

            if (delayLine.Count <= perChannelDelay)
            {
                // We need to start writing into the buffer
                var channelBuffer = new float[perChannelCount];
                VectorMath.vscale(buffer, offset + fromChannel, totalChannels, channelBuffer, 0, 1, decay, perChannelRead);
                delayLine.Write(channelBuffer, 0, perChannelRead);

            }
            // if we have come to the end of the original sample
            // we need to keep playing out the decay buffer until the end
            if (samplesRead < count)
            {
                var endOfOriginalRead = count - samplesRead;
                var leftOver = endOfOriginalRead / totalChannels;
                var bufferCount = (int)Math.Min(leftOver, delayLine.Count);
                var channelBuffer = new float[perChannelCount];
                delayLine.Read(channelBuffer, endOfOriginalRead, bufferCount);
                VectorMath.vcopy(channelBuffer, 0, 1, output, outOffset + endOfOriginalRead + toChannel, totalChannels, bufferCount);
                totalSamplesWritten += bufferCount;
            }
            return totalSamplesWritten;
        }


    }
}
