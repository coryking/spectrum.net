using CorySignalGenerator.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                Console.WriteLine("Playing {0}", _notes[noteNumber]);
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

        public bool IsNotePlaying(int noteNumber)
        {
            return _activeNotes.ContainsKey(noteNumber);
        }
    }
}
