using CorySignalGenerator.Models;
using CorySignalGenerator.SampleProviders;
using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace CorySignalGenerator.Sequencer
{
    /// <summary>
    /// Controls the notes that are currently active
    /// </summary>
    public class VoiceController : ISampleProvider
    {
        private Dictionary<MidiNote, INote> _activeNotes = new Dictionary<MidiNote, INote>();
        private object _lock = new object();
        private ISampler _sampler;
        private ExposedMixingSampleProvider _mixer;
        private bool isSustainOn;

        public VoiceController(ISampler sampler)
        {
            _sampler = sampler;
            _mixer = new ExposedMixingSampleProvider(sampler.WaveFormat);
            _mixer.ReadFully = true;
        }

        public INote NoteOn(MidiNote note, float velocity)
        {
            lock (_lock)
            {
                if (IsNotePlaying(note))
                    return _activeNotes[note];

                var generatedNote = _sampler.GetNote(note, velocity);
                
                if (isSustainOn)
                    generatedNote.SustainOn();

                _activeNotes.Add(note, generatedNote);
                _mixer.AddMixerInput(generatedNote);
                return generatedNote;
            }

        }

        public INote NoteOff(MidiNote note)
        {
            lock (_lock)
            {
                // IM done for the day....
                if (!IsNotePlaying(note))
                    return null;

                var provider = _activeNotes[note];
                provider.NoteOff();
                _activeNotes.Remove(note);
                return provider;
            }

        }

        protected bool IsNotePlaying(MidiNote note)
        {
            return _activeNotes.ContainsKey(note);
        }

        public void SustainOn()
        {
            lock (_lock)
            {
                isSustainOn = true;
                _activeNotes.Values.ForEach(x => x.SustainOn());
            }
        }
        public void SustainOff()
        {
            lock (_lock)
            {
                _mixer.Sources.OfType<INote>().ForEach(x => x.SustainOff());
                isSustainOn = false;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _activeNotes.Clear();
                _mixer.RemoveAllMixerInputs();
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                return _mixer.Read(buffer, offset, count);
            }
        }

        public WaveFormat WaveFormat
        {
            get { return _mixer.WaveFormat; }
        }
    }
}
