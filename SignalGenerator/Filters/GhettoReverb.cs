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
using System.Threading;
namespace CorySignalGenerator.Filters
{
    public class GhettoReverb : Effect
    {
        /// <summary>
        /// Get the maximum reverb delay
        /// </summary>
        private const int MaxDelaySamples = 44100;
        private List<DelayLine> _delayLines;
        private EqFilter _eqFilter;

        public GhettoReverb(ISampleProvider source) : base(source)
        {
        }

        protected override void Init()
        {
            _delayLines = new List<DelayLine>();
            _eqFilter = new EqFilter(SampleRate, Channels) { HighPassCutoff = HighPassCutoffFrequency, LowPassCutoff = LowPassCutoffFrequency };
            if (Channels == 1)
            {
                _delayLines.Add(new DelayLine(MaxDelaySamples));
            }
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
            // Zero out the destination buffer just to be safe
            Array.Clear(buffer, 0, count);

            // Read into the buffer
            var samplesRead = Source.Read(buffer, offset, count);

            // if there is nothing to do... bail now.
            if (SampleDelay < 1 || Decay < 0.01f)
                return samplesRead;

            var readFromConvolver = 0;
            var isEnd = (samplesRead < count);
            var reverbBuffer = new float[count];

            // Read from the delay line...
            if (Channels==1)
            {
                readFromConvolver = _delayLines[0].Read(reverbBuffer, 0, count);
            }
            else if(Channels==2)
            {
                var tempBus = new float[4][];
                var reads = new int[4];

                Parallel.ForEach(_delayLines, (line, state, index) =>
                {
                    tempBus[index] = new float[count];
                    reads[index] = line.Read(tempBus[index], 0, count);
                });
                foreach (var item in reads)
                {
                    readFromConvolver = Math.Max(item, readFromConvolver);
                }
                // Combine all the streams
                VectorMath.vadd(tempBus[0], 0, 1, tempBus[1], 0, 1, tempBus[2], 0, 1, tempBus[3], 0, 1, reverbBuffer, 0, 1, readFromConvolver);

            }

            // Apply our eq....
            if(_eqFilter != null && UseEQ)
                _eqFilter.Transform(reverbBuffer, 0, readFromConvolver);

            Parallel.ForEach(_delayLines, (delayLine) =>
            {
                // feed it back into the delay line /w scaling
                delayLine.Accumulate(reverbBuffer, 0, readFromConvolver);
                // add the buffer in too...
                delayLine.Write(buffer, offset, readFromConvolver);
            });

            // Add the reverb buffer into the output stream
            VectorMath.vadd(reverbBuffer, 0, 1, buffer, offset, 1, buffer, offset, 1, readFromConvolver);

            return readFromConvolver;
        }

        #region Properties


        #region Property UseEQ
        private bool _useEQ = true;

        /// <summary>
        /// Sets and gets the UseEQ property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool UseEQ
        {
            get
            {
                return _useEQ;
            }
            set
            {
                Set(ref _useEQ, value);
            }
        }
        #endregion
		

        #region Property HighPassCutoffFrequency
        private float _highPassCutoffFrequency = 200f;

        /// <summary>
        /// Sets and gets the HighPassCutoffFrequency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float HighPassCutoffFrequency
        {
            get
            {
                return _highPassCutoffFrequency;
            }
            set
            {
                Set(ref _highPassCutoffFrequency, value);
                SetEqFrequencies();
            }
        }
        #endregion
		
        #region Property LowPassCutoffFrequency
        private float _lowPassCutoffFrequency = 18000f;

        /// <summary>
        /// Sets and gets the LowPassCutoffFrequency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float LowPassCutoffFrequency
        {
            get
            {
                return _lowPassCutoffFrequency;
            }
            set
            {
                Set(ref _lowPassCutoffFrequency, value);
                SetEqFrequencies();
            }
        }
        #endregion
		


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

        #endregion // properties
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
            return (int)(ms * SampleRate / 1000) / Channels; // ms * samples/s * s/ms = samples
        }

        public void SetEqFrequencies()
        {
            if (_eqFilter == null)
                return;
            _eqFilter.HighPassCutoff = HighPassCutoffFrequency;
            _eqFilter.LowPassCutoff = LowPassCutoffFrequency;
        }

        protected override void HandlePropertyChanged(string propertyName)
        {
            base.HandlePropertyChanged(propertyName);
            SetDelayLineParams();
        }
    }
}
