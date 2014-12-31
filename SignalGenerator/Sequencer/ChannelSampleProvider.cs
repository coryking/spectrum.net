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

namespace CorySignalGenerator.Sequencer
{
    public class ChannelSampleProvider : ISampleProvider
    {
        private ISoundModel _noteProvider;

        public ChannelSampleProvider(ISoundModel noteProvider)
        {
            WaveFormat = noteProvider.WaveFormat;
            Tracker = new NoteTracker();
            _noteProvider = noteProvider;
            RebuildMixer();
        }

        private MixingSampleProvider _mixer;
        private void RebuildMixer()
        {
            _mixer = new MixingSampleProvider(WaveFormat);
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
            var provider = Tracker.StopNote(noteNumber);
            if (provider is IStoppableSample)
            {
                ((IStoppableSample)provider).Stop();
            }
        }

        public void PlayNote(int noteNumber, int velocity)
        {
            var provider = Tracker.PlayNote(noteNumber, (freq) =>
            {
                return _noteProvider.GetProvider(freq, velocity, noteNumber);
            });
            if(provider != null)
                _mixer.AddMixerInput(provider);
        }

        public NoteTracker Tracker { get; set; }

    }
}
