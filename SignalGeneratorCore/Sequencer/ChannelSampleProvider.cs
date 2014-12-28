using CorySynth;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SignalGeneratorCore.Models;
using SignalGeneratorCore.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGeneratorCore.Sequencer
{
    public class ChannelSampleProvider : ISampleProvider
    {
        private IProviderModel _noteProvider;

        public ChannelSampleProvider(IProviderModel noteProvider, int sampleRate, int channels)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
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
            if (provider is BasicNote)
            {
                ((BasicNote)provider).StopNote();
            }
        }

        public void PlayNote(int noteNumber, int velocity)
        {
            var provider = Tracker.PlayNote(noteNumber, (freq) =>
            {
                return _noteProvider.GetProvider(freq, velocity, WaveFormat.SampleRate, WaveFormat.Channels);
            });
            if(provider != null)
                _mixer.AddMixerInput(provider);
        }

        public NoteTracker Tracker { get; set; }

    }
}
