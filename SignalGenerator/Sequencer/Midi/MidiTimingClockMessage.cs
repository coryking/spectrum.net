using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    public class MidiTimingClockMessage :IMidiMessage
    {
        public MidiTimingClockMessage(TimeSpan timeStamp)
        {
            Timestamp = timeStamp;
            Type = MidiMessageType.TimingClock;
        }

#if NETFX_CORE
        public MidiTimingClockMessage(WindowsPreview.Devices.Midi.MidiTimingClockMessage message)
        {
            
            Timestamp = message.Timestamp;
            Type = (MidiMessageType)message.Type;
        }
#endif

        public TimeSpan Timestamp
        {
            get;
            private set;
        }

        public MidiMessageType Type
        {
            get;
            private set;
        }
    }
}
