using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sounds
{
    public interface ISoundModel
    {
        WaveFormat WaveFormat { get; }

        /// <summary>
        /// Initalize any samples
        /// </summary>
        void InitSamples();

        ISampleProvider GetProvider(float frequency, int velocity);
    }
}
