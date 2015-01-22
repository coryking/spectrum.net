// .NET port of Super-pitch JS effect included with Cockos REAPER
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class SuperPitch : JSNetEffect
    {
       

        public SuperPitch(ISampleProvider source) :base(source)
        {
        
        }

        protected override Func<JSNet.Effect> CreateEffectInstance
        {
            get
            {
                return () =>
                {
                    return new JSNet.SuperPitch();
                };
            }
        }

        protected override void SetEffectParams()
        {
            SetEffectSlider(0, PitchCents);
            SetEffectSlider(1, PitchSemitones);
            SetEffectSlider(2, PitchOctaves);
            SetEffectSlider(3, WindowSize);
            SetEffectSlider(4, OverlapSize);
            SetEffectSlider(5, WetMix);
            SetEffectSlider(6, DryMix);
        }
        

        #region properties

        public override string Name { get { return "Super Pitch"; } }

        #region Property PitchCents
        private float _pitchCents = 0.0f;

        /// <summary>
        /// Pitch Adjust (cents)
        /// </summary>
        public float PitchCents
        {
            get
            {
                return _pitchCents;
            }
            set
            {
                Set(ref _pitchCents, value);
            }
        }
        #endregion


        #region Property PitchSemitones
        private float _pitchSemitones = 0.0f;

        /// <summary>
        /// Pitch Adjust (semitones)
        /// </summary>
        public float PitchSemitones
        {
            get
            {
                return _pitchSemitones;
            }
            set
            {
                Set(ref _pitchSemitones, value);
            }
        }
        #endregion


        #region Property PitchOctaves
        private float _pitchOctaves = 0f;

        /// <summary>
        /// Pitch Adjust (octaves)
        /// </summary>
        public float PitchOctaves
        {
            get
            {
                return _pitchOctaves;
            }
            set
            {
                Set(ref _pitchOctaves, value);
            }
        }
        #endregion


        #region Property WindowSize
        private float _windowSize = 50f;

        /// <summary>
        /// Window Size (ms)
        /// </summary>
        public float WindowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                Set(ref _windowSize, value);
            }
        }
        #endregion


        #region Property OverlapSize
        private float _overlapSize = 20f;

        /// <summary>
        /// Overlap Size (ms)
        /// </summary>
        public float OverlapSize
        {
            get
            {
                return _overlapSize;
            }
            set
            {
                Set(ref _overlapSize, value);
            }
        }
        #endregion


        #region Property WetMix
        private float _wetMix = 0f;

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
                Set(ref _wetMix, value);
            }
        }
        #endregion


        #region Property DryMix
        private float _dryMix = -120f;

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
                Set(ref _dryMix, value);
            }
        }
        #endregion

        #endregion



    }
}
