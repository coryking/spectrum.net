using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// Factory for creating effects.
    /// </summary>
    public interface IEffectFactory
    {
        /// <summary>
        /// Name of this effect factory
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Generates a new <see cref="IEffect"/>
        /// </summary>
        /// <param name="source">The source being modified by this effect</param>
        /// <returns>A new effect</returns>
        IEffect GetEffect(ISampleProvider source);

    }

    public interface IEnvelopeEffectFactory
    {
        /// <summary>
        /// Name of the envelope effect
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Get the envelope effect
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEnvelopeEffect GetEnvelope(ISampleProvider source);
    }
}
