using System;
using System.Collections.Generic;
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
        public HarmonicProfile()
        {

        }

        #region Properties
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
		

        #endregion

        
        #region Property AplitudeMultiplierType
        private AmplitudeMultiplierType _amplitude_multipler_type = AmplitudeMultiplierType.OFF;

        /// <summary>
        /// Sets and gets the AplitudeMultiplierType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public AmplitudeMultiplierType AplitudeMultiplierType
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
		
    }
}
