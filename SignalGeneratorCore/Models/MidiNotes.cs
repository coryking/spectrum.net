using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySynth.Models
{
    public struct MidiNote
    {
        private static string[] noteNames = new string[]
        {
            "C", // 0
            "C#",
            "D",
            "D#",
            "E",
            "F",
            "F#",
            "G",
            "G#",
            "A",
            "A#",
            "B", // 11
        };


        public MidiNote( int number, double a4freq){

            double exponent = (number - 69.0) / 12.0;
            Frequency = a4freq * Math.Pow(2.0d, exponent);

            int octive = number / 12;
            int note = number % 12;

            Name = String.Format("{0}{1}", noteNames[note], octive - 1);

            Number = number;

        }

        public double Frequency;
        public String Name;
        public int Number;
        public override string ToString()
        {
            return String.Format("{2}: {0} {1}hz", Name, Frequency, Number);
        }
    }

    public class MidiNotes : Dictionary<int, MidiNote>
    {
        public static MidiNotes GenerateNotes(float a4Freq=440.0f)
        {
            var notes = new MidiNotes();
            for (var i = 0; i < 128; i++)
            {
                var note = new MidiNote(i, a4Freq);
                notes[i] = note;
            }
            return notes;
        }

    }
}
