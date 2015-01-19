using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using CorySignalGenerator.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Models;

namespace CorySignalGenerator.Sounds
{
    public class SignalGeneretedSound: PropertyChangeModel, ISoundModel
    {
        public SignalGeneretedSound(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }


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
		

        public string Name { get { return "Signal Generated"; } }


        public WaveFormat WaveFormat
        {
            get;
            private set;
        }

        public ISampleProvider GetProvider(float frequency, int velocity, int noteNumber)
        {
            return new SignalGenerator(WaveFormat.SampleRate, WaveFormat.Channels)
            {
                Frequency = frequency,
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
            get;
            private set;
        }
    }
}
