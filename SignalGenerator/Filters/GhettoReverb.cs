using CorySignalGenerator.Utils;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
using CorySignalGenerator.Dsp;
namespace CorySignalGenerator.Filters
{
    public class GhettoReverb : Effect
    {
        /// <summary>
        /// Get the maximum reverb delay
        /// </summary>
        private const int MaxDelaySamples = 44100;
        private List<DelayLine> _delayLines;

        public GhettoReverb(ISampleProvider source) : base(source)
        {
        }

        protected override void Init()
        {
            _delayLines = new List<DelayLine>();
            for (int i = 0; i < Channels; i++)
            {
                _delayLines.Add(new DelayLine(MaxDelaySamples));
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

            var readFromConvolver = 0;
            // Read from the delay line...
            for (int i = 0; i < Channels; i++)
            {
                var delayLine = _delayLines[i];
                readFromConvolver = delayLine.ConvolveDelayLine(buffer, offset, count, samplesRead, Decay, SampleDelay, i, Channels);
            }

            return readFromConvolver;
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
