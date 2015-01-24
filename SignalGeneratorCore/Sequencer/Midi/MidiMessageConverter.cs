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
            IMidiMessage result = null;
            switch (windowsMidiMessage.Type)
            {
                case WindowsPreview.Devices.Midi.MidiMessageType.ActiveSensing:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.ChannelPressure:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.Continue:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.ControlChange:
                    result = new MidiControlChangeMessage((WindowsPreview.Devices.Midi.MidiControlChangeMessage)windowsMidiMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.MidiTimeCode:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.None:
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.NoteOff:
                    result = new MidiNoteOffMessage((WindowsPreview.Devices.Midi.MidiNoteOffMessage)windowsMidiMessage);
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.NoteOn:
                    result = new MidiNoteOnMessage((WindowsPreview.Devices.Midi.MidiNoteOnMessage)windowsMidiMessage);
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
                    break;
                case WindowsPreview.Devices.Midi.MidiMessageType.TuneRequest:
                    break;
                default:
                    break;
            }
            return result;
        }

    }
}
