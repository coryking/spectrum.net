using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// Defines a "live" effect
    /// </summary>
    public interface IEffect : ISourcedSampleProvider
    {
        /// <summary>
        /// Name of this effect
        /// </summary>
        String Name { get; }

        /// <summary>
        /// What order should this effect be?  Lower is closer to the source, higher is further away from the source
        /// </summary>
        int Order { get; }

    }

    /// <summary>
    /// Envelope effects are ones that stop the whole thing from playing
    /// </summary>
    public interface IEnvelopeEffect :IEffect
    {
        /// <summary>
        /// 
        /// </summary>
        void Stop();
    }
}
