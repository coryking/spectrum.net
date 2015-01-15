using CorySignalGenerator.Utils;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
namespace CorySignalGenerator.Filters
{
    public class GhettoReverb : Effect
    {
        /// <summary>
        /// Get the maximum reverb delay
        /// </summary>
        private const int MaxDelaySamples = 44100;
        private List<CircularBuffer> _delayLines;

        public GhettoReverb(ISampleProvider source) : base(source)
        {
        }

        protected override void Init()
        {
            _delayLines = new List<CircularBuffer>();
            for (int i = 0; i < Channels; i++)
            {
                _delayLines.Add(new CircularBuffer(MaxDelaySamples));
            }
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            // Zero out the destination buffer just to be safe
            Array.Clear(buffer, 0, count);

            // Read into the buffer
            var samplesRead = Source.Read(buffer, offset, count);
            // if there is nothing to do... bail now.
            if (SampleDelay < 10 || Decay < 0.01f)
                return samplesRead;

            var totalSamplesRead = samplesRead;

            var perChannelCount = count/Channels;
            // Read from the delay line...
            for (int i = 0; i < Channels; i++)
            {
                var delayLine = _delayLines[i];
                var sourceOffset = offset + i; // offset of this channel in the original buffer
                // if the amount of stuff in the buffer is more than our delay, start using it!
                if(delayLine.Count > SampleDelay)
                {
                    var channelBuffer = new float[perChannelCount];
                    delayLine.Read(channelBuffer, 0, perChannelCount);
                    
                    // Add the delay line back into the output...
                    VectorMath.vadd(buffer, sourceOffset, Channels, channelBuffer, 0, 1, buffer, sourceOffset, Channels, perChannelCount);
                }
                
                if (delayLine.Count <= SampleDelay)
                {
                    // We need to start writing into the buffer
                    var channelBuffer = buffer.TakeChannel(i, perChannelCount, Channels);
                    channelBuffer.Scale(Decay);
                    delayLine.Write(channelBuffer, 0, perChannelCount);

                }
                // if we have come to the end of the original sample
                // we need to keep playing out the decay buffer until the end
                if (samplesRead < count)
                {
                    var endOfOriginalRead = count - samplesRead;
                    var leftOver = endOfOriginalRead / Channels;
                    var bufferCount = (int)Math.Min(leftOver, delayLine.Count);
                    var channelBuffer = new float[perChannelCount];
                    delayLine.Read(channelBuffer, endOfOriginalRead, bufferCount);
                    channelBuffer.InterleaveChannel(buffer, i, endOfOriginalRead+offset, bufferCount, Channels);
                    totalSamplesRead += bufferCount;
                }
            }

            return totalSamplesRead;
        }

        private float _delay;
        /// <summary>
        /// The reverb delay, in milliseconds
        /// </summary>
        public float Delay
        {
            get { return _delay; }
            set { Set(ref _delay, value); }
        }

        private float _decay;
        /// <summary>
        /// Gets / Sets the decay for the reverb.  Values should be between 0 (no reverb) and 1 (no decay)
        /// </summary>
        public float Decay
        {
            get { return _decay; }
            set { Set(ref _decay, value); }
        }
        /// <summary>
        /// Gets the number of samples to delay something
        /// </summary>
        public int SampleDelay
        {
            get
            {
                // ms * samples/s * s/ms = samples
                return (int)(Delay * SampleRate / 1000);
            }
        }

    }
}
