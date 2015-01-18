﻿using CorySignalGenerator.Models;
using CorySignalGenerator.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
namespace CorySignalGenerator
{
    public class NoteTracker
    {
        private Dictionary<int, ISampleProvider> _activeNotes = new Dictionary<int, ISampleProvider>();
        private MidiNotes _notes;
        private object _lock = new object();
        public NoteTracker()
        {
            _notes = MidiNotes.GenerateNotes();
        }

        public ISampleProvider PlayNote(int noteNumber, Func<float,ISampleProvider> generator)
        {
            lock (_lock)
            {
                if (IsNotePlaying(noteNumber))
                    return _activeNotes[noteNumber];

                Debug.WriteLine("Playing {0}", _notes[noteNumber]);
                var sampleProvider = generator.Invoke((float)_notes[noteNumber].Frequency);
                _activeNotes.Add(noteNumber, sampleProvider);
                return sampleProvider;
            }

        }

        public ISampleProvider StopNote(int noteNumber)
        {
            lock (_lock)
            {
                if (!IsNotePlaying(noteNumber))
                    return null;

                var provider = _activeNotes[noteNumber];
                _activeNotes.Remove(noteNumber);
                return provider;
            }
        }

        public void PedalDown()
        {
            lock (_lock)
            {
                _activeNotes.Values.OfType<ISustainable>().ForEach(x => x.SustainOn());
            }
        }
        public void PedalUp()
        {
            lock (_lock)
            {
                _activeNotes.Values.OfType<ISustainable>().ForEach(x => x.SustainOff());
            }
        }

        public bool IsNotePlaying(int noteNumber)
        {
            return _activeNotes.ContainsKey(noteNumber);
        }
    }
}
