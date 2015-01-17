using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Dsp
{
    public class EqFilter : CorySignalGenerator.Models.PropertyChangeModel
    {
        public const float Q = 0.5f;
        private NAudio.Dsp.BiQuadFilter[] _highPassFilters;
        private NAudio.Dsp.BiQuadFilter[] _lowPassFilters;

        public EqFilter(int sampleRate, int channels) : base()
        {
            SampleRate = sampleRate;
            Channels = channels;

            _highPassFilters = new NAudio.Dsp.BiQuadFilter[Channels];
            _lowPassFilters = new NAudio.Dsp.BiQuadFilter[Channels];
            this.CreateFilters();
        }

        #region Properties
        #region Property HighPassCutoff
        private float _highPassCutoff = 200f;

        /// <summary>
        /// Sets and gets the HighPassCutoff property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float HighPassCutoff
        {
            get
            {
                return _highPassCutoff;
            }
            set
            {
                Set(ref _highPassCutoff, value);
            }
        }
        #endregion


        #region Property LowPassCutoff
        private float _lowPassCutoff = 18000f;

        /// <summary>
        /// Sets and gets the LowPassCutoff property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float LowPassCutoff
        {
            get
            {
                return _lowPassCutoff;
            }
            set
            {
                Set(ref _lowPassCutoff, value);
            }
        }
        #endregion
		


        public int SampleRate { get; private set; }

        public int Channels { get; private set; }

        #endregion

        public int Transform(float[] buffer, int offset, int count)
        {
            for (var channel = 0; channel < Channels; channel++)
            {
                for (int i = offset + channel; i < count; i += Channels)
                {
                    buffer[i] = _highPassFilters[channel].Transform(buffer[i]);
                    buffer[i] = _lowPassFilters[channel].Transform(buffer[i]);

                }
            }

            return count;
        }
        protected override void HandlePropertyChanged(string propertyName)
        {
            base.HandlePropertyChanged(propertyName);
            CreateFilters();
        }

        private void CreateFilters()
        {
            for (int i = 0; i < Channels; i++)
            {
                _highPassFilters[i] = NAudio.Dsp.BiQuadFilter.HighPassFilter(SampleRate, HighPassCutoff, Q);
                _lowPassFilters[i] = NAudio.Dsp.BiQuadFilter.LowPassFilter(SampleRate, LowPassCutoff, Q);
                
            }
        }
        
    }
}
