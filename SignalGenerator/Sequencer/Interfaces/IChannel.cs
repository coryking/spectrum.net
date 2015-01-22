using CorySignalGenerator.Sequencer.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// Defines a single midi channel
    /// </summary>
    public interface IChannel : ISampleProvider
    {
        /// <summary>
        /// Gets the midi channel number
        /// </summary>
        int ChannelNumber { get; }

        /// <summary>
        /// Do something with a midi message
        /// </summary>
        /// <param name="message">the midi message</param>
        void ProcessMidiMessage(IMidiMessage message);

    }
}
