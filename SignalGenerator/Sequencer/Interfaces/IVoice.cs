using CorySignalGenerator.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// Defines a voice, which is usually a <see cref="ISampler"/> and a chain of effects under it
    /// </summary>
    public interface IVoice : ISampleProvider
    {
        /// <summary>
        /// Name of this voice
        /// </summary>
        String Name { get; }


        /// <summary>
        /// A note has been played
        /// </summary>
        /// <param name="note">The note</param>
        /// <param name="velocity">The lineraly defined velocity (scale 0.0-1.0) </param>
        void NoteOn(MidiNote note, float velocity);

        void NoteOff(MidiNote note);

        /// <summary>
        /// Sustain should activate
        /// </summary>
        void SustainOn();

        /// <summary>
        /// Deactivate sustain
        /// </summary>
        void SustainOff();

    }
}
