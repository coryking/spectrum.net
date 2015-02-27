using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    /// <summary>
    /// Any Midi Event
    /// </summary>
    public class MidiInputMessageEventArgs
    {
        public MidiInputMessageEventArgs( IMidiMessage message)
        {
            Message = message;
        }

        public IMidiMessage Message { get; private set; }

    }
}
