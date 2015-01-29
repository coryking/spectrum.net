using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class FlangerEffect :JSNetEffect
    {
        public FlangerEffect(WaveFormat format) : base(format) { }

        public FlangerEffect(ISampleProvider source)
            : base(source)
        {

        }


        protected override Func<JSNet.Effect> CreateEffectInstance
        {
            get {
                return () =>
                {
                    return new JSNet.Flanger();
                };
            }
        }

        protected override void SetEffectParams()
        {
            //AddSlider(6, 0, 200, 1, "length (ms)");
            //AddSlider(-120, -120, 6, 1, "feedback (dB)");
            //AddSlider(-6, -120, 6, 1, "wet mix (dB)");
            //AddSlider(-6, -120, 6, 1, "dry mix (dB)");
            //AddSlider(0.6f, 0.001f, 100, 0.1f, "rate (hz)");

            SetEffectSlider(0, this.Length);
            SetEffectSlider(1, this.Feedback);
            SetEffectSlider(2, this.WetMix);
            SetEffectSlider(3, this.DryMix);
            SetEffectSlider(4, this.Rate);
        }
        public override string Name { get { return "Chorus"; } }

        #region properties


        #region Property Rate
        private float _rate = 0.6f;

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
                Set(ref _rate, value,0.001f,100);
            }
        }
        #endregion
		

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
                Set(ref _wetMix, value, -100, 12);
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
                Set(ref _dryMix, value, -100, 12);
            }
        }
        #endregion




        #region Property Feedback
        private float _feedback = -120f;

        /// <summary>
        /// Sets and gets the Feedback property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Feedback
        {
            get
            {
                return _feedback;
            }
            set
            {
                Set(ref _feedback, value,-120,6);
            }
        }
        #endregion
		

        #region Property Length
        private float _length = 6f;

        /// <summary>
        /// Sets and gets the Length property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Length
        {
            get
            {
                return _length;
            }
            set
            {
                Set(ref _length, value, 0,200);
            }
        }
        #endregion
		



        #endregion

    }
}
