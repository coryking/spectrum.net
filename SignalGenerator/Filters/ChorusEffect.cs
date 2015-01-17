using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class ChorusEffect : JSNetEffect
    {
        public ChorusEffect(ISampleProvider source)
            : base(source)
        {

        }


        protected override Func<JSNet.Effect> CreateEffectInstance
        {
            get {
                return () =>
                {
                    return new JSNet.Chorus();
                };
            }
        }

        protected override void SetEffectParams()
        {
            //AddSlider(15, 1, 250, 1, "chorus length (ms)");
            //AddSlider(1, 1, 8, 1, "number of voices");
            //AddSlider(0.5f, 0.1f, 16, 0.1f, "rate (hz)");
            //AddSlider(0.7f, 0, 1, 0.1f, "pitch fudge factor");
            //AddSlider(-6, -100, 12, 1, "wet mix (dB)");
            //AddSlider(-6, -100, 12, 1, "dry mix (dB)");
            SetEffectSlider(0, this.ChorusLength);
            SetEffectSlider(1, this.Voices);
            SetEffectSlider(2, this.Rate);
            SetEffectSlider(3, this.PitchFudgeFactor);
            SetEffectSlider(4, this.WetMix);
            SetEffectSlider(5, this.DryMix);

        }
        #region Properties

        #region Property WetMix
        private float _wetMix = -6f;

        /// <summary>
        /// Wet Mix (dB)
        /// </summary>
        public float WetMix
        {
            get
            {
                return _wetMix;
            }
            set
            {
                Set(ref _wetMix, value,-100,12);
            }
        }
        #endregion

        #region Property DryMix
        private float _dryMix = -6f;

        /// <summary>
        /// Dry Mix (dB)
        /// </summary>
        public float DryMix
        {
            get
            {
                return _dryMix;
            }
            set
            {
                Set(ref _dryMix, value,-100,12);
            }
        }
        #endregion

        #region Property PitchFudgeFactor
        private float _pitchFudgeFactor = 0.7f;

        /// <summary>
        /// Sets and gets the PitchFudgeFactor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float PitchFudgeFactor
        {
            get
            {
                return _pitchFudgeFactor;
            }
            set
            {
                Set(ref _pitchFudgeFactor, value,0f,1f);
            }
        }
        #endregion
		
        #region Property Rate
        private float _rate = 0.5f;

        /// <summary>
        /// Sets and gets the Rate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Rate
        {
            get
            {
                return _rate;
            }
            set
            {
                Set(ref _rate, value,0.1f, 16f);
            }
        }
        #endregion

        #region Property Voices
        private int _voices = 1;

        /// <summary>
        /// Sets and gets the Voices property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Voices
        {
            get
            {
                return _voices;
            }
            set
            {
                Set(ref _voices, value,1,8);
            }
        }
        #endregion
		
        #region Property ChorusLength
        private float _chorusLength = 15;

        /// <summary>
        /// Sets and gets the ChorusLength property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float ChorusLength
        {
            get
            {
                return _chorusLength;
            }
            set
            {
                Set(ref _chorusLength, value,1,250);
            }
        }
        #endregion
        #endregion
    }
}
