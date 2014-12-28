using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SignalGeneratorCore.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGeneratorCore.Models
{
    public class BasicNoteModel: IProviderModel
    {
        public BasicNoteModel()
        {

        }

        public ISampleProvider GetProvider(float frequency, int velocity, int sampleRate, int channels)
        {
            return new BasicNote(sampleRate, channels)
            {
                Frequency=frequency,
                Velocity=velocity,
                Type=Type,
                ReleaseSeconds=ReleaseSeconds,
                AttackSeconds=AttackSeconds
            };
        }

        public SignalGeneratorType Type { get; set; }

        public float ReleaseSeconds { get; set; }
        public float AttackSeconds { get; set; }
    }
}
