using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class GhettoReverb : Effect
    {


        public GhettoReverb(ISampleProvider source) : base(source)
        {
        }

        protected override void Init()
        {
            _decayBuffer = new Queue<float>(); ;
            
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = Source.Read(buffer, offset, count);
            // if there is nothing to do... bail now.
            if (SampleDelay < 10 || Decay < 0.01f)
                return samplesRead;

            var totalSamplesRead = samplesRead;

            for (var n = 0; n < samplesRead; n++)
            {
                if (DecayBuffer.Count > SampleDelay * WaveFormat.Channels)
                {
                    buffer[n + offset] += DecayBuffer.Dequeue();
                    if (WaveFormat.Channels == 2 && DecayBuffer.Count > 0)
                        buffer[n + offset] += (DecayBuffer.Peek() * 0.5f);
                }
                if (DecayBuffer.Count <= SampleDelay * WaveFormat.Channels)
                    DecayBuffer.Enqueue(buffer[n + offset] * Decay);

            }
            // if we have come to the end of the original sample
            // we need to keep playing out the decay buffer until the end
            if (samplesRead < count)
            {
                var bufferCount = Math.Min(count - samplesRead, DecayBuffer.Count);
                for (var n = 0; n < bufferCount; n++)
                {
                    buffer[n + samplesRead + offset] = DecayBuffer.Dequeue();
                }
                totalSamplesRead += bufferCount;
            }
            return totalSamplesRead;
        }

        private Queue<float> _decayBuffer;

        protected Queue<float> DecayBuffer
        {
            get { return _decayBuffer; }
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
