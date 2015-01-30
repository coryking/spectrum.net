using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    public sealed class MidiControlChangeMessage : IMidiMessage
    {
        public MidiControlChangeMessage(byte channel, byte controller, byte controlValue)
        {
            Channel = channel;
            Controller = controller;
            ControlValue = controlValue;
            Type = MidiMessageType.ControlChange;
        }

#if NETFX_CORE
        public MidiControlChangeMessage(WindowsPreview.Devices.Midi.MidiControlChangeMessage message) : this(message.Channel,message.Controller,message.ControlValue)
        {
            Timestamp = message.Timestamp;
        }
#endif
        

        public byte Channel { get; private set; }
        public byte Controller { get; private set; }
        public byte ControlValue { get; private set; }
        public TimeSpan Timestamp { get; private set; }
        public MidiMessageType Type { get; private set; }
    }
}
