﻿using CorySignalGenerator.Utils;
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
            if (Channels == 1)
                _delayLines.Add(new DelayLine(MaxDelaySamples));
            else if (Channels == 2)
            {
                _delayLines.Add(new DelayLine(MaxDelaySamples));
                _delayLines.Add(new DelayLine(MaxDelaySamples));
                _delayLines.Add(new DelayLine(MaxDelaySamples));
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
            if (SampleDelay < 1 || Decay < 0.01f)
                return samplesRead;

            var readFromConvolver = 0;
            // Read from the delay line...
            if (Channels==1)
            {
                readFromConvolver = _delayLines[0].ConvolveDelayLine(buffer, offset, buffer,offset, count, samplesRead, Decay, SampleDelay, 0,0, Channels);
            }
            else if(Channels==2)
            {
                var tempBusLL = new float[MaxDelaySamples];
                var tempBusLR = new float[MaxDelaySamples];
                var tempBusRR = new float[MaxDelaySamples];
                var tempBusRL = new float[MaxDelaySamples];
                
                // Left -> Left
                readFromConvolver = _delayLines[0].ConvolveDelayLine(buffer, offset, tempBusLL, offset, count, samplesRead, Decay, SampleDelay, 0,0, Channels);
                // Left -> Right
                readFromConvolver = _delayLines[1].ConvolveDelayLine(buffer, offset, tempBusLR, offset, count, samplesRead, SecondaryDecay, SampleSecondaryDelayLeft + SampleDelay, 0,1, Channels);



                // Right -> Right
                readFromConvolver = _delayLines[2].ConvolveDelayLine(buffer, offset, tempBusRR, 0, count, samplesRead, Decay, SampleDelay, 1,1, Channels);
                // Right -> Left
                readFromConvolver = _delayLines[3].ConvolveDelayLine(buffer, offset, tempBusRL, 0, count, samplesRead, SecondaryDecay, SampleSecondaryDelayRight + SampleDelay, 1,0, Channels);

                // add back in the right channel
                VectorMath.vadd(tempBusLL, 0, 1, tempBusLR, 0, 1, tempBusRR, 0, 1, tempBusRL, 0, 1, buffer, 0, 1, readFromConvolver);

            }
            for (int i = 0; i < Channels; i++)
            {
                var delayLine = _delayLines[i];
                
            }

            return readFromConvolver;
        }

     
        private float _secondaryDecay = 0.25f;

        /// <summary>
        /// Controls the amount of decay that is fed into the other channel.
        /// </summary>
        public float SecondaryDecay
        {
            get
            {
                return _secondaryDecay;
            }
            set
            {
                Set(ref _secondaryDecay, value);
            }
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

        private float _delay;
        /// <summary>
        /// The reverb delay, in milliseconds
        /// </summary>
        public float Delay
        {
            get { return _delay; }
            set
            {
                SampleDelay = GetSampleDelay(value);
                Set(ref _delay, value);
            }
        }


        #region Property SecondaryDelayLeft
        private float _secondaryDelayLeft = 15f;

        /// <summary>
        /// Sets / Gets the additional decay in milliseconds to apply the secondary decay coming from the left into the right.
        /// </summary>
        public float SecondaryDelayLeft
        {
            get
            {
                return _secondaryDelayLeft;
            }
            set
            {
                SampleSecondaryDelayLeft = GetSampleDelay(value);
                Set(ref _secondaryDelayLeft, value);
            }
        }
        #endregion


        #region Property SecondaryDelayRight
        private float _secondaryDelayRight = 18f;

        /// <summary>
        /// Sets / Gets the additional decay in milliseconds to apply the secondary decay coming from the right into the left.
        /// </summary>
        public float SecondaryDelayRight
        {
            get
            {
                return _secondaryDelayRight;
            }
            set
            {
                SampleSecondaryDelayRight = GetSampleDelay(value);
                Set(ref _secondaryDelayRight, value);
            }
        }
        #endregion
		
        /// <summary>
        /// Gets the number of samples to delay something
        /// </summary>
        public int SampleDelay
        {
            get;
            set;
        }

        public int SampleSecondaryDelayLeft { get; set; }
        public int SampleSecondaryDelayRight { get; set; }

        protected int GetSampleDelay(float ms)
        {
            return (int)(ms * SampleRate / 1000); // ms * samples/s * s/ms = samples
        }
    }
}
