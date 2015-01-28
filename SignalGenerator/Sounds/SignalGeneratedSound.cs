﻿using NAudio.Wave;
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
            return new ChangableSignalGenerator(WaveFormat.SampleRate, WaveFormat.Channels, this)
            {
                Frequency = (float)note.Frequency,
                Type = Type,
            };
        }

        protected override bool SupportsVelocity
        {
            get { return true; }
        }
    }
}
