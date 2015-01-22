using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using CorySignalGenerator.Models;
using System.Collections.ObjectModel;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// Defines a sampler--i.e. something can can make notes
    /// </summary>
    public interface ISampler
    {
        WaveFormat WaveFormat { get; }

        /// <summary>
        /// Get a note.  This method is basically the whole point of the sampler...
        /// </summary>
        /// <param name="note">Midi note</param>
        /// <param name="velocity">Linear note velocity (0.0 - 1.0)</param>
        /// <returns></returns>
        INote GetNote(MidiNote note, float velocity);

        /// <summary>
        /// Name of this sampler
        /// </summary>
        String Name { get; }

    }
}
