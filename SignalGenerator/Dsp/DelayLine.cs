using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
using System.Diagnostics;
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

        private int _sampleDelay;
        public int SampleDelay
        {
            get
            {
                return _sampleDelay;
            }
            set
            {
                if (_sampleDelay != value)
                    ChangeSampleDelay(value, _sampleDelay);
                _sampleDelay = value;
            }
        }

        private void ChangeSampleDelay(int newDelay, int oldDelay)
        {
            Debug.Assert(delayLine != null);
            if(newDelay > delayLine.Count)
            {
                var buffer = new float[MaxDelaySamples];
                delayLine.Write(buffer, 0, newDelay - delayLine.Count);
            }
            else
            {
                delayLine.Advance(delayLine.Count - newDelay);
            }
        }

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
        /// Add a buffer into the delay line and optionally advance the write pointer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="endOfSample">If true, signals this is the end of the sample.</param>
        /// <returns>Total number of array elements read from input buffer</returns>
        public int Write(float[] buffer, int offset, int count, bool endOfSample=false)
        {
            return Accumulate(buffer, offset, count, endOfSample, true);
        }

        /// <summary>
        /// Add a buffer into the delay line without advancing the delay line
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>Total number of array elements read from input buffer</returns>
        public int Accumulate(float[] buffer, int offset, int count, bool endOfSample=false, bool advanceWritePointer=false)
        {
            var perChannelCount = count / Channels;
            var channelBuffer = new float[MaxDelaySamples];

            VectorMath.vscale(buffer, offset + FromChannel, Channels, channelBuffer, 0, 1, Decay, perChannelCount);
            delayLine.Accumulate(channelBuffer, 0, perChannelCount,advanceWritePointer);
            
            if (endOfSample)
                phase = DelayLinePhase.End;

            return count;
        }



        public void Reset()
        {
            phase = DelayLinePhase.ReadWrite;
            delayLine.Reset();
        }
    }
}
