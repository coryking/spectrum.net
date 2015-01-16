using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
namespace CorySignalGenerator.Dsp
{
    internal enum DelayLinePhase
    {
        ReadWrite,
        End
    }

    public class DelayLine
    {
        private const int MaxDelaySamples = 44100;

        private CircularBuffer delayLine;

        private DelayLinePhase phase;

        public DelayLine(int maxBuffer)
        {
            delayLine = new CircularBuffer(maxBuffer);
            phase = DelayLinePhase.ReadWrite;
        }

        public int FromChannel { get; set; }
        public int ToChannel { get; set; }
        public int Channels { get; set; }

        public int SampleDelay { get; set; }

        public float Decay { get; set; }

        /// <summary>
        /// Write from delay line into a buffer
        /// </summary>
        /// <param name="output"></param>
        /// <param name="outOffset"></param>
        /// <param name="count"></param>
        /// <returns>Total number of array elements written into output buffer</returns>
        public int Read(float[] output, int outOffset, int count)
        {
            
            var perChannelCount = count / Channels;
            var totalSamplesWritten = perChannelCount; // this is the total samples written

            // if the amount of stuff in the buffer is more than our delay, start using it!
            if ((delayLine.Count >= (SampleDelay + perChannelCount)) || phase == DelayLinePhase.End)
            {
                var channelBuffer = new float[MaxDelaySamples];
                var maxRead = (int)Math.Min(perChannelCount, delayLine.Count);
                delayLine.Read(channelBuffer, 0, maxRead);

                // Copy the delay line into the output buffer, overwriting whatever is there.
                VectorMath.vcopy(channelBuffer, outOffset, 1, output, outOffset + ToChannel, Channels, maxRead);
                totalSamplesWritten = Math.Max(maxRead, totalSamplesWritten);
            }

            return totalSamplesWritten * Channels;
        }

        /// <summary>
        /// Read from a buffer into the delay line
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="endOfSample">If true, signals this is the end of the sample.</param>
        /// <returns>Total number of array elements read from input buffer</returns>
        public int Write(float[] buffer, int offset, int count, bool endOfSample=false)
        {
            var perChannelCount = count / Channels;
            var channelBuffer = new float[MaxDelaySamples];

            // Add in our delay if we need it
            if (delayLine.Count < SampleDelay)
                delayLine.Write(channelBuffer, 0, SampleDelay - delayLine.Count);

            // We need to start writing into the buffer
            VectorMath.vscale(buffer, offset + FromChannel, Channels, channelBuffer, 0, 1, Decay, perChannelCount);
            delayLine.Write(channelBuffer, 0, perChannelCount);

            if (endOfSample)
                phase = DelayLinePhase.End;

            return count;
        }


    }
}
