using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    public class SignalGeneratorNote : AdsrNote
    {

        public SignalGeneratorNote(WaveFormat waveFormat) : 
            base(waveFormat)
        {
         
        }

        public SignalGeneratorType Type { get; set; }
        public float Frequency { get; set; }

        protected override NAudio.Wave.ISampleProvider GetSampler()
        {
            return new SignalGenerator(WaveFormat.SampleRate, WaveFormat.Channels)
            {
                Frequency = Frequency,
                Type = Type
            };
        }
    }
}
