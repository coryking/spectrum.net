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
using CorySignalGenerator.Sequencer;
using System.Diagnostics;

namespace CorySignalGenerator.Sounds
{
    public class SignalGeneretedSound: NoteSampler
    {
        public SignalGeneretedSound(WaveFormat waveFormat) :base()
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


        public override string Name { get { return "Signal Generated"; } }


        public override WaveFormat WaveFormat
        {
            get;
            protected set;
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

        protected override ISampleProvider GenerateNote(MidiNote note)
        {
            var generator = new ChangableSignalGenerator(WaveFormat.SampleRate, 1, this)
            {
                Frequency = (float)note.Frequency,
                Type = Type,
            };
            var panAmount = (note.Number - 63) / 128f;
            Debug.WriteLine("Pan Amount: {0}", panAmount);
            var panned = new PanningSampleProvider(generator)
            {
                Pan = panAmount,
            };
            return panned;

        }

        protected override bool SupportsVelocity
        {
            get { return true; }
        }
    }
}
