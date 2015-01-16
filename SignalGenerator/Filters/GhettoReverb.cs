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
            if (Channels == 1)
                _delayLines.Add(new DelayLine(MaxDelaySamples));
            else if (Channels == 2)
            {
                _delayLines.Add(new DelayLine(MaxDelaySamples) { FromChannel = 0, ToChannel = 0, Channels = Channels });
                _delayLines.Add(new DelayLine(MaxDelaySamples) { FromChannel = 0, ToChannel = 1, Channels = Channels });
                _delayLines.Add(new DelayLine(MaxDelaySamples) { FromChannel = 1, ToChannel = 1, Channels = Channels });
                _delayLines.Add(new DelayLine(MaxDelaySamples) { FromChannel = 1, ToChannel = 0, Channels = Channels });
            }
            
        }

        private void SetDelayLineParams()
        {
            if(Channels ==1 ){
                _delayLines[0].Decay = Decay;
                _delayLines[0].SampleDelay = SampleDelay;
            }
            else if (Channels == 2)
            {
                _delayLines[0].Decay = Decay;
                _delayLines[2].Decay = Decay;
                _delayLines[1].Decay = SecondaryDecay;
                _delayLines[3].Decay = SecondaryDecay;

                _delayLines[0].SampleDelay = SampleDelay;
                _delayLines[2].SampleDelay = SampleDelay;
                _delayLines[1].SampleDelay = SampleSecondaryDelayRight + SampleDelay; 
                _delayLines[3].SampleDelay = SampleSecondaryDelayLeft + SampleDelay;
            }
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            SetDelayLineParams();
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
                readFromConvolver = _delayLines[0].ConvolveDelayLine(buffer, offset, samplesRead, buffer, offset, count);
            }
            else if(Channels==2)
            {
                var tempBusL = new float[MaxDelaySamples];
                var tempBusR = new float[MaxDelaySamples];
                var amountToCopy = 0;
                
                // Left -> Left
                var samplesWritten = _delayLines[0].ConvolveDelayLine(buffer, offset, samplesRead, tempBusL, offset, count);
                amountToCopy = Math.Max(samplesWritten, amountToCopy);
                // Left -> Right
                samplesWritten = _delayLines[1].ConvolveDelayLine(buffer, offset, samplesRead, tempBusL, offset, count);
                amountToCopy = Math.Max(samplesWritten, amountToCopy);

                // Right -> Right
                samplesWritten = _delayLines[2].ConvolveDelayLine(buffer, offset, samplesRead, tempBusR, 0, count);
                amountToCopy = Math.Max(samplesWritten, amountToCopy);

                // Right -> Left
                samplesWritten = _delayLines[3].ConvolveDelayLine(buffer, offset, samplesRead, tempBusR, 0, count);
                amountToCopy = Math.Max(samplesWritten, amountToCopy);

                // add everything back together
                VectorMath.vadd(tempBusL, 0, 1, tempBusR, 0, 1, buffer, 0, 1, buffer,0,1, amountToCopy * 2);
                readFromConvolver = amountToCopy * 2;

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
