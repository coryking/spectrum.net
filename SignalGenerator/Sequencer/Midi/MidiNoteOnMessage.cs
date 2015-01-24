using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    public class MidiNoteOnMessage :IMidiMessage
    {
        public MidiNoteOnMessage(byte channel, byte note, byte velocity)
        {
            Channel = channel;
            Note = note;
            Velocity = velocity;
            Type = MidiMessageType.NoteOn;
        }
        #if NETFX_CORE
        public MidiNoteOnMessage(WindowsPreview.Devices.Midi.MidiNoteOnMessage message)
        {
            Channel = message.Channel;
            Note = message.Note;
            Velocity = message.Velocity;
            Channel = message.Channel;
            Timestamp = message.Timestamp;
            Type = MidiMessageType.NoteOn;
        }
#endif

        public byte Channel { get; private set; }
        public byte Note { get; private set; }
        
        public TimeSpan Timestamp { get; private set; }
        public MidiMessageType Type { get; private set; }
        public byte Velocity { get; private set; }
    }
}
