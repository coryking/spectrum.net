using CorySignalGenerator.Models;
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
        private bool SustainOn;
        private Dictionary<int, ISampleProvider> _activeNotes = new Dictionary<int, ISampleProvider>();
        private Queue<IStoppableSample> SamplesToStop;
        private MidiNotes _notes;
        private object _lock = new object();
        public NoteTracker()
        {
            _notes = MidiNotes.GenerateNotes();
            SamplesToStop = new Queue<IStoppableSample>();
        }

        public ISampleProvider PlayNote(int noteNumber, Func<float,ISampleProvider> generator)
        {
            lock (_lock)
            {
                if (IsNotePlaying(noteNumber))
                    return _activeNotes[noteNumber];

                Debug.WriteLine("Playing {0}", _notes[noteNumber]);
                var sampleProvider = generator.Invoke((float)_notes[noteNumber].Frequency);
                if (SustainOn && sampleProvider is ISustainable)
                    ((ISustainable)sampleProvider).SustainOn();
                _activeNotes.Add(noteNumber, sampleProvider);
                return sampleProvider;
            }

        }


        public ISampleProvider StopNote(int noteNumber)
        {
            // IM done for the day....
            if (!IsNotePlaying(noteNumber))
                return null;

            var provider = _activeNotes[noteNumber];
                
            lock (_lock)
            {
                if (provider is IStoppableSample)
                {
                    var stopProvider = (IStoppableSample)provider;
                    stopProvider.Stop();
                    _activeNotes.Remove(noteNumber);

                }
                else
                {
                    _activeNotes.Remove(noteNumber);
                }
                
            }

               
                return provider;
            
        }

        public void PedalDown()
        {
            Debug.WriteLine("Somebody Pressed The Pedal");
            lock (_lock)
            {
                _activeNotes.Values.OfType<ISustainable>().ForEach(x => x.SustainOn());
                SustainOn = true;
            }
        }
        public void PedalUp()
        {
            Debug.WriteLine("Somebody Released The Pedal");
            lock (_lock)
            {
                _activeNotes.Values.OfType<ISustainable>().ForEach(x => x.SustainOff());
                SustainOn = false;
            }
        }

        public bool IsNotePlaying(int noteNumber)
        {
            return _activeNotes.ContainsKey(noteNumber);
        }

        public void Reset()
        {
            lock (_lock)
            {
                _activeNotes.Clear();
            }
        }
    }
}
