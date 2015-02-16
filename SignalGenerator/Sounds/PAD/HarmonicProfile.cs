using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sounds.PAD
{
    public enum FrequencyBaseType
    {
        Gauss = 0,
        Square = 1,
        DoubleExp = 2
    }

    public enum AmplitudeMultiplierType
    {
        OFF=0,
        Gauss=1,
        Sine=2,
        Flat=3
    }

    public enum AmplitudeMultiplerMode
    {
        Sum=0,
        Mult=1,
        Div1=2,
        Div2=3,
    }

    public class HarmonicProfile : Models.PropertyChangeModel
    {
        public const int PROFILE_SIZE = 512;


        private bool isProfileDirty = false;
        private object _locker = new object();

        public HarmonicProfile()
        {
            Debug.WriteLine("In HarmonicProfile Constructor");

        }

        /// <summary>
        /// Get the harmonic profile
        /// </summary>
        /// <param name="output">An output buffer</param>
        /// <returns>The bandwidth adjustment, if any</returns>
        public float GetHarmonicProfile(float[] output)
        {
            lock (_locker)
            {
                if (Profile == null || isProfileDirty)
                {
                    if (Profile == null)
                        Profile = new float[PROFILE_SIZE];
                    else
                        Array.Clear(Profile, 0, PROFILE_SIZE);

                    BWAdjust = BuildProfile(Profile, PROFILE_SIZE);
                }
                isProfileDirty = false;
                Array.Copy(Profile, output, Profile.Length);
                return BWAdjust;
            }
        }



        protected float BuildProfile(float[] profile, int profilesize)
        {
            var supersample = 16;
            var basepar = FloatMath.pow(2.0f, (1.0f - BaseWidth / 127.0f) * 12.0f);
            var freqmult = FloatMath.floor(FloatMath.pow(2.0f, FrequencyMultiplier / 127.0f * 5.0f) + 0.000001f);
            var modfreq = FloatMath.floor(FloatMath.pow(2.0f, ModulatorFrequency / 127.0f * 5.0f) + 0.000001f);
            var modpar1 = FloatMath.pow(BaseWidth / 127.0f, 4.0f) * 5.0f / FloatMath.sqrt(modfreq);

            float amppar1 = FloatMath.pow(2.0f, FloatMath.pow(AmplitudeMultiplierParam1 / 127.0f, 2.0f) * 10.0f) - 0.999f;
            float amppar2 = (1.0f - AmplitudeMultiplierParam2 / 127.0f) * 0.998f + 0.001f;

            var width = FloatMath.pow(150.0f / (Size + 22.0f), 2.0f);

            for (int i = 0; i < profilesize * supersample; i++)
            {
                var makezero = false;
                var x = i * 1.0f / (profilesize * (float)supersample);
                float origx = x;
                //do the sizing (width)
                x = (x - 0.5f) * width + 0.5f;
                if (x < 0.0f)
                {
                    x = 0.0f;
                    makezero = true;
                }
                else if (x > 1.0f)
                {
                    x = 1.0f;
                    makezero = true;
                }
                // if you want to do anything with the profile... do it here
                float x_before_freq_mult = x;
                x *= freqmult;

                //do the modulation of the profile
                x += FloatMath.sin(x_before_freq_mult * 3.1415926f * modfreq) * modpar1;
                x = FloatMath.mod(x + 1000.0f, 1.0f) * 2.0f - 1.0f;

                float f;
                switch (BaseType)
                {
                    case CorySignalGenerator.Sounds.PAD.FrequencyBaseType.Square:
                        f = FloatMath.exp(-(x * x) * basepar);
                        if (f < 0.4f)
                            f = 0.0f;
                        else
                            f = 1.0f;
                        break;
                    case CorySignalGenerator.Sounds.PAD.FrequencyBaseType.DoubleExp:
                        f = FloatMath.exp(-(FloatMath.abs(x)) * FloatMath.sqrt(basepar));

                        break;
                    default: // gaussian
                        f = FloatMath.exp(-(x * x) * basepar);
                        break;
                }

                if (makezero)
                    f = 0.0f;

                float amp = 1.0f;
                origx = origx * 2.0f - 1.0f;

                switch (AmplitudeMultiplierType)
                {
                    case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplierType.Gauss:
                        amp = FloatMath.exp(-(origx * origx) * 10.0f * amppar1);
                        break;
                    case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplierType.Sine:
                        amp = 0.5f * (1.0f + FloatMath.cos(3.1415926f * origx * FloatMath.sqrt(amppar1 * 4.0f + 1.0f)));
                        break;
                    case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplierType.Flat:
                        amp = 1.0f / (FloatMath.pow(origx * (amppar1 * 2.0f + 0.8f), 14.0f) + 1.0f);
                        break;
                    default:
                        break;
                }

                float finalsmp = f;
                if (AmplitudeMultiplierType != PAD.AmplitudeMultiplierType.OFF)
                {
                    switch (AmplitudeMultiplerMode)
                    {
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Sum:
                            finalsmp = amp * (1.0f - amppar2) + finalsmp * amppar2;

                            break;
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Mult:
                            finalsmp *= amp * (1.0f - amppar2) + amppar2;

                            break;
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Div1:
                            finalsmp = finalsmp / (amp + FloatMath.pow(amppar2, 4.0f) * 20.0f + 0.0001f);
                            break;
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Div2:
                            finalsmp = amp / (finalsmp + FloatMath.pow(amppar2, 4.0f) * 20.0f + 0.0001f);
                            break;
                        default:
                            break;
                    }
                }
                profile[i / supersample] += finalsmp / supersample;

            }

            FrequencyUtils.NormalizeToOne(profile, profilesize);

            if (!AutoScale)
                return 0.5f;

            //compute the estimated perceived bandwidth
            float sum = 0.0f;
            int index;
            for (index = 0; index < profilesize / 2 - 2; ++index)
            {
                sum += profile[index] * profile[index] + profile[profilesize - index - 1] * profile[profilesize - index - 1];
                if (sum >= 4.0f)
                    break;
            }

            float result = 1.0f - 2.0f * index / (float)profilesize;
            return result;
        }
        protected override void HandlePropertyChanged(string propertyName)
        {
            base.HandlePropertyChanged(propertyName);
            isProfileDirty = true;
        }

        #region Properties

        protected float[] Profile { get; set; }

        protected float BWAdjust { get; set; }

        #region Property AutoScale
        private bool _autoScale = false;

        /// <summary>
        /// Sets and gets the AutoScale property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AutoScale
        {
            get
            {
                return _autoScale;
            }
            set
            {
                Set(ref _autoScale, value);
            }
        }
        #endregion
		

        #region Property BaseType
        private FrequencyBaseType _baseType = FrequencyBaseType.Gauss;

        /// <summary>
        /// Sets and gets the BaseType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FrequencyBaseType BaseType
        {
            get
            {
                return _baseType;
            }
            set
            {
                Set(ref _baseType, value);
            }
        }
        #endregion


        #region Property BaseWidth
        private int _baseWidth = 64;

        /// <summary>
        /// Sets and gets the BaseWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int BaseWidth
        {
            get
            {
                return _baseWidth;
            }
            set
            {
                Set(ref _baseWidth, value);
            }
        }
        #endregion


        #region Property Size
        private int _size = 0;

        /// <summary>
        /// Refered to as "width"
        /// </summary>
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                Set(ref _size, value);
            }
        }
        #endregion
		


        #region Property FrequencyMultiplier
        private int _frequencyMultipler = 0;

        /// <summary>
        /// Sets and gets the FrequencyMultiplier property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FrequencyMultiplier
        {
            get
            {
                return _frequencyMultipler;
            }
            set
            {
                Set(ref _frequencyMultipler, value);
            }
        }
        #endregion


        #region Property Str
        private int _str = 0;

        /// <summary>
        /// known as "modulator.par1
        /// </summary>
        public int Str
        {
            get
            {
                return _str;
            }
            set
            {
                Set(ref _str, value);
            }
        }
        #endregion


        #region Property ModulatorFrequency
        private int _modulatorFrequency = 0;

        /// <summary>
        /// Otherwise known as modulator.freq
        /// </summary>
        public int ModulatorFrequency
        {
            get
            {
                return _modulatorFrequency;
            }
            set
            {
                Set(ref _modulatorFrequency, value);
            }
        }
        #endregion

        
        #region Property AplitudeMultiplierType
        private AmplitudeMultiplierType _amplitude_multipler_type = AmplitudeMultiplierType.OFF;

        /// <summary>
        /// Sets and gets the AplitudeMultiplierType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public AmplitudeMultiplierType AmplitudeMultiplierType
        {
            get
            {
                return _amplitude_multipler_type;
            }
            set
            {
                Set(ref _amplitude_multipler_type, value);
            }
        }
        #endregion

        #region Property AmplitudeMultiplerMode
        private AmplitudeMultiplerMode _amplitude_multipler_mode = AmplitudeMultiplerMode.Mult;

        /// <summary>
        /// Sets and gets the AmplitudeMultiplerMode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public AmplitudeMultiplerMode AmplitudeMultiplerMode
        {
            get
            {
                return _amplitude_multipler_mode;
            }
            set
            {
                Set(ref _amplitude_multipler_mode, value);
            }
        }
        #endregion
		
		

        #region Property AmplitudeMultiplierParam1
        private int _amplitude_multiplier_par1 = 0;

        /// <summary>
        /// Sets and gets the AmplitudeMultiplerParam1 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int AmplitudeMultiplierParam1
        {
            get
            {
                return _amplitude_multiplier_par1;
            }
            set
            {
                Set(ref _amplitude_multiplier_par1, value);
            }
        }
        #endregion


        #region Property AmplitudeMultiplierParam2
        private int _amplitude_multiplier_par2 = 0;

        /// <summary>
        /// Sets and gets the AmplitudeMultiplierParam2 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int AmplitudeMultiplierParam2
        {
            get
            {
                return _amplitude_multiplier_par2;
            }
            set
            {
                Set(ref _amplitude_multiplier_par2, value);
            }
        }
        #endregion
        #endregion


    }
}
