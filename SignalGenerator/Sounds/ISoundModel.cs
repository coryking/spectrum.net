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

        /// <summary>
        /// Is the sample table fully loaded?
        /// </summary>
        bool IsSampleTableLoaded { get; }

        /// <summary>
        /// What is the name of this sound model
        /// </summary>
        string Name { get; }

        ISampleProvider GetProvider(float frequency, int velocity, int noteNumber);
    }
}
