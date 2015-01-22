using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    /// <summary>
    /// Defines a midi message
    /// </summary>
    public interface IMidiMessage
    {
        TimeSpan Timestamp { get; }
        MidiMessageType Type { get; }
    }
}
