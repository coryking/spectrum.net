using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    /// <summary>
    /// Convert from WindowsPreview midi messages
    /// </summary>
    public static class MidiMessageConverter
    {
        public static IMidiMessage ToMidiMessage(WindowsPreview.Devices.Midi.IMidiMessage windowsMidiMessage)
        {
            Type messageType = null;
            switch (windowsMidiMessage.Type)
            {
                case WindowsPreview.Devices.Midi.MidiMessageType.ActiveSensing:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.ChannelPressure:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.Continue:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.ControlChange:
                    messageType = typeof(MidiControlChangeMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.MidiTimeCode:
                    messageType = typeof(MidiTimeCodeMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.None:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.NoteOff:
                    messageType = typeof(MidiNoteOffMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.NoteOn:
                    messageType = typeof(MidiNoteOnMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.PitchBendChange:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.PolyphonicKeyPressure:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.ProgramChange:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.SongPositionPointer:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.SongSelect:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.Start:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.Stop:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.SystemExclusive:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.SystemReset:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.TimingClock:
                    messageType = typeof(MidiTimingClockMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.TuneRequest:
                    break;
                default:
                    break;
            }

            if (messageType != null)
            {
                return (IMidiMessage)Activator.CreateInstance(messageType, windowsMidiMessage);
            }
            else
            {
                return null;
            }
        }

    }
}
