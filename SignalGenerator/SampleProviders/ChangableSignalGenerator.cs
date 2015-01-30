using CorySignalGenerator.Sounds;
using CorySignalGenerator.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    public class ChangableSignalGenerator : ISampleProvider
    {
        private SignalGenerator _generator;
        private SignalGeneretedSound _eventSource;
        private float _originalFrequency;

        private int _sampleRate;
        private int _channels;

        public ChangableSignalGenerator(int sampleRate, int channels, SignalGeneretedSound eventSource)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            _generator = new SignalGenerator(sampleRate, channels);
            // Todo: This needs to use some kind of weak event handler...  this method has us living as long we our event source
            //_eventSource = eventSource;
            //_eventSource.PropertyChanged += eventSource_PropertyChanged;

        }

        public float Pitch { get; set; }

        public float Frequency { get; set; }

        public SignalGeneratorType Type { get; set; }


        public int Read(float[] buffer, int offset, int count)
        {
            var frequency = FrequencyUtils.ScaleFrequency(Frequency, Pitch, 12);

            // If we have to rebuild the type, we need to rebuild the generator
            if (_generator.Type != Type)
            {
                _generator = new SignalGenerator(_sampleRate, _channels);
                _generator.Type = Type;
            }

            _generator.Frequency = frequency;
            return _generator.Read(buffer, offset, count);
        }

        public WaveFormat WaveFormat
        {
            get { return _generator.WaveFormat; }
        }
    }
}
