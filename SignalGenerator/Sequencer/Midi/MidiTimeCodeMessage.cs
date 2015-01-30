using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    public class MidiTimeCodeMessage : IMidiMessage
    {
        public MidiTimeCodeMessage(byte frameType, byte values, TimeSpan timeStamp)
        {
            FrameType = frameType;
            Values = values;
            Timestamp = timeStamp;
            Type = MidiMessageType.MidiTimeCode;
        }

#if NETFX_CORE
        public MidiTimeCodeMessage(WindowsPreview.Devices.Midi.MidiTimeCodeMessage message)
        {
            FrameType = message.FrameType;
            Values = message.Values;
            Timestamp = message.Timestamp;
            Type = (MidiMessageType)message.Type;
        }
#endif

        public byte FrameType { get; private set; }

        public byte Values { get; private set; }

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
