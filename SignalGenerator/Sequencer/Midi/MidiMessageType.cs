using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Midi
{
    public enum MidiMessageType
    {
        None = 0,
        NoteOff = 128,
        NoteOn = 144,
        PolyphonicKeyPressure = 160,
        ControlChange = 176,
        ProgramChange = 192,
        ChannelPressure = 208,
        PitchBendChange = 224,
        SystemExclusive = 240,
        MidiTimeCode = 241,
        SongPositionPointer = 242,
        SongSelect = 243,
        TuneRequest = 246,
        TimingClock = 248,
        Start = 250,
        Continue = 251,
        Stop = 252,
        ActiveSensing = 254,
        SystemReset = 255,
    }
}
