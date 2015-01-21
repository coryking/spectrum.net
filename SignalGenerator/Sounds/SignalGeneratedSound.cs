using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using CorySignalGenerator.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Models;
using CorySignalGenerator.Utils;

namespace CorySignalGenerator.Sounds
{
    public class SignalGeneretedSound: PropertyChangeModel, ISoundModel
    {
        public SignalGeneretedSound(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }


        #region Property IsEnabled
        private bool _isEnabled = false;

        /// <summary>
        /// Gets or sets if this sound model is enabled
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                Set(ref _isEnabled, value);
            }
        }
        #endregion
		


        #region Property Type
        private SignalGeneratorType _type = SignalGeneratorType.Sin;

        /// <summary>
        /// Sets and gets the Type property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SignalGeneratorType Type
        {
            get
            {
                return _type;
            }
            set
            {
                Set(ref _type, value);
            }
        }
        #endregion


        
        #region Property Pitch
        private float _pitch = 0f;

        /// <summary>
        /// Gets / Sets the pitch of this note on a scale of -12 to 12 (-12 is one octave below, 12 is an octave above)
        /// </summary>
        public float Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                Set(ref _pitch, value, -12, 12);
            }
        }
        #endregion
		

        public string Name { get { return "Signal Generated"; } }


        public WaveFormat WaveFormat
        {
            get;
            private set;
        }

        public ISampleProvider GetProvider(float frequency, int velocity, int noteNumber)
        {
            if (!IsEnabled)
                return null;

            return new SignalGenerator(WaveFormat.SampleRate, WaveFormat.Channels)
            {
                Frequency = FrequencyUtils.ScaleFrequency(frequency, Pitch, 12),
                Type = Type,
           };

        }


        public void InitSamples()
        {
            // Don't do anything.  we generate stuff on the fly
            return;
        }



        public bool IsSampleTableLoaded
        {
            get { return true; }
        }
    }
}
