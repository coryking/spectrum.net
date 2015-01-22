using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    public class MidiNoteOffMessage : IMidiMessage
    {

        public MidiNoteOffMessage(byte channel, byte note, byte velocity)
        {
            Channel = channel;
            Note = note;
            Velocity = velocity;
            Type = MidiMessageType.NoteOff;
        }
#if NETFX_CORE
        public MidiNoteOffMessage(WindowsPreview.Devices.Midi.MidiNoteOffMessage message)
        {
            Channel = message.Channel;
            Note = message.Note;
            Channel = message.Channel;
            Timestamp = message.Timestamp;
            Type = MidiMessageType.NoteOff;
        }
#endif

        public byte Channel { get; private set; }
        public byte Note { get; private set; }
        public TimeSpan Timestamp { get; private set; }
        public MidiMessageType Type { get; private set; }
        public byte Velocity { get; private set; }

    }
}
