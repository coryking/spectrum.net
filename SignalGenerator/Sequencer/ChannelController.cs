using CorySignalGenerator.Sequencer.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using CorySignalGenerator.Models;

namespace CorySignalGenerator.Sequencer
{
    public class ChannelController
    {
        public ChannelController(ObservableCollection<IVoice> voices, ObservableCollection<IEffect> effects, int channelNumber)
        {
            Voices = voices;
            Effects = effects;
            Notes = MidiNotes.GenerateNotes();
        }

        protected int ChannelNumber { get; set; }

        protected ObservableCollection<IVoice> Voices { get; private set; }
        protected ObservableCollection<IEffect> Effects { get; private set; }

        protected MidiNotes Notes { get; private set; }
        
        public void ProcessMidiMessage(Midi.IMidiMessage message)
        {
            switch (message.Type)
            {
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.None:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.NoteOff:
                    var offNote = (Midi.MidiNoteOffMessage)message;
                    HandleNote(offNote.Channel,offNote.Note,offNote.Velocity);
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.NoteOn:
                    var onNote=(Midi.MidiNoteOnMessage)message;
                    HandleNote(onNote.Channel,onNote.Note, onNote.Velocity);
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.PolyphonicKeyPressure:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ControlChange:
                     var msg = (Midi.MidiControlChangeMessage)message;
                    ControlChange(msg.Channel,msg.Controller,msg.ControlValue);
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ProgramChange:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ChannelPressure:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.PitchBendChange:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SystemExclusive:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.MidiTimeCode:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SongPositionPointer:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SongSelect:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.TuneRequest:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.TimingClock:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.Start:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.Continue:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.Stop:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ActiveSensing:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SystemReset:
                    break;
                default:
                    break;
            }
        }

        private void HandleNote(byte channel, byte noteNumber, byte velocity)
        {
            if (channel != ChannelNumber)
                return;
            var note = Notes[noteNumber];
            if (velocity == 0)
                Voices.ForEach(x => x.NoteOff(note));
            else
                Voices.ForEach(x => x.NoteOn(note, velocity/127f)); // For now, we just do a linear velocity

        }

        private void HandlePedal(byte controlValue)
        {
            if (controlValue == 0)
            {
                Voices.ForEach(x => x.SustainOff());
            }
            else
            {
                Voices.ForEach(x => x.SustainOn());
            }

        }

        protected void ControlChange(byte channel, byte controller, byte controlValue)
        {
            if (ChannelNumber != channel)
                return;

            switch (controller)
            {
                case 64:
                    HandlePedal(controlValue);
                    break;
                default:
                    break;
            }
        }
    }
}
