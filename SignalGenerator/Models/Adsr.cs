﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{
    public class Adsr : PropertyChangeModel, CorySignalGenerator.Models.IWrapSampleProvider
    {

        public Adsr()
        {
           
        }

        public ISampleProvider WrapProvider(ISampleProvider source)
        {
            return new SampleProviders.AdsrSampleProvider(source, AttackMs, DecayMs, SustainLevel, ReleaseMs);

        }

        #region Properties

        #region Property SustainLevel
        private float _sustainLevel = 1f;

        /// <summary>
        /// Sets and gets the SustainLevel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float SustainLevel
        {
            get
            {
                return _sustainLevel;
            }
            set
            {
                Set(ref _sustainLevel, value);
            }
        }
        #endregion
		

        #region Property DecayMs
        private float _decayMs = 10f;

        /// <summary>
        /// Sets and gets the DecayMs property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float DecayMs
        {
            get
            {
                return _decayMs;
            }
            set
            {
                Set(ref _decayMs, value);
            }
        }
        #endregion
		
        #region Property AttackMs
        private float _attackMs = 50f;

        /// <summary>
        /// Sets and gets the AttackMs property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float AttackMs
        {
            get
            {
                return _attackMs;
            }
            set
            {
                Set(ref _attackMs, value);
            }
        }
        #endregion
		
        #region Property ReleaseMs
        private float _releaseMs = 50f;

        /// <summary>
        /// Sets and gets the ReleaseMs property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float ReleaseMs
        {
            get
            {
                return _releaseMs;
            }
            set
            {
                Set(ref _releaseMs, value);
            }
        }
        #endregion
        #endregion

    }
}
