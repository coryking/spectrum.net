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

        public DelayLine(int maxBuffer)
        {
            delayLine = new CircularBuffer(maxBuffer);
        }
        public int ConvolveDelayLine(float[] buffer, int offset, int count, int samplesRead, float decay, int sampleDelay, int channel, int totalChannels)
        {
            var totalSamplesRead = samplesRead;
            var perChannelCount = count / totalChannels;

            var sourceOffset = offset + channel; // offset of this channel in the original buffer
            // if the amount of stuff in the buffer is more than our delay, start using it!
            if (delayLine.Count > sampleDelay)
            {
                var channelBuffer = new float[perChannelCount];
                delayLine.Read(channelBuffer, 0, perChannelCount);

                // Add the delay line back into the output...
                VectorMath.vadd(buffer, sourceOffset, totalChannels, channelBuffer, 0, 1, buffer, sourceOffset, totalChannels, perChannelCount);
            }

            if (delayLine.Count <= sampleDelay)
            {
                // We need to start writing into the buffer
                var channelBuffer = buffer.TakeChannel(channel, perChannelCount, totalChannels);
                channelBuffer.Scale(decay);
                delayLine.Write(channelBuffer, 0, perChannelCount);

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
                channelBuffer.InterleaveChannel(buffer, channel, endOfOriginalRead + offset, bufferCount, totalChannels);
                totalSamplesRead += (bufferCount * totalChannels);
            }
            return totalSamplesRead;
        }


    }
}
