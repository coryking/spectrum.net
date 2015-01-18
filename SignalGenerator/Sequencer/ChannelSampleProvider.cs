using CorySignalGenerator;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using CorySignalGenerator.Models;
using CorySignalGenerator.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Sounds;
using MoreLinq;
namespace CorySignalGenerator.Sequencer
{
    public class ChannelSampleProvider : ISampleProvider
    {
        private ISoundModel _noteProvider;
        protected bool _sustainOn;

        public ChannelSampleProvider(ISoundModel noteProvider)
        {
            WaveFormat = noteProvider.WaveFormat;
            Tracker = new NoteTracker();
            _noteProvider = noteProvider;
            RebuildMixer();
        }

        private ExposedMixingSampleProvider _mixer;
        private void RebuildMixer()
        {
            _mixer = new ExposedMixingSampleProvider(WaveFormat);
            _mixer.ReadFully = true;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _mixer.Read(buffer, offset, count);
        }

        public WaveFormat WaveFormat
        {
            get;
            private set;
        }

        public void StopNote(int noteNumber)
        {
            Tracker.StopNote(noteNumber);
        }

        public void PlayNote(int noteNumber, int velocity)
        {
            var provider = Tracker.PlayNote(noteNumber, (freq) =>
            {
                return _noteProvider.GetProvider(freq, velocity, noteNumber);
            });

            // If the sustain pedal is on, make sure all the new providers know about it
            if (_sustainOn && provider is ISustainable)
                ((ISustainable)provider).SustainOn();
            
            if(provider != null)
                _mixer.AddMixerInput(provider);
        }

        public NoteTracker Tracker { get; set; }

        private void HandlePedal(byte channel, byte controlValue)
        {
            if (controlValue == 0)
            {
                _sustainOn = false;
                _mixer.Sources.OfType<ISustainable>().ForEach(x => x.SustainOff());
            }
            else
            {
                _sustainOn = true; ;
                _mixer.Sources.OfType<ISustainable>().ForEach(x => x.SustainOn());
            }

        }

        public void ControlChange(byte channel, byte controller, byte controlValue)
        {
            switch (controller)
            {
                case 64:
                    HandlePedal(channel, controlValue);
                    break;
                default:
                    break;
            }
        }

        public void Reset()
        {
            _mixer.RemoveAllMixerInputs();
            Tracker.Reset();
        }
    }
}
