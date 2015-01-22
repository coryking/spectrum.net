using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// Defines a note that will be played
    /// </summary>
    public interface INote : ISampleProvider
    {
        /// <summary>
        /// Signal the end of a note
        /// </summary>
        void NoteOff();

        /// <summary>
        /// Sustain this note
        /// </summary>
        void SustainOn();

        /// <summary>
        /// "Unsustain" this note
        /// </summary>
        void SustainOff();
    }
}
