using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{

    public enum KeyColor
    {
        White,
        Black
    };

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

            Octave = (number / 12) - 1;
            int note = number % 12;

            Name = String.Format("{0}{1}", noteNames[note], Octave);

            // a stilly huristic that works...
            KeyColor = noteNames[note].Length == 1 ? KeyColor.White : KeyColor.Black;

            Number = number;

        }

        /// <summary>
        /// Frequency of the note
        /// </summary>
        public double Frequency;
        /// <summary>
        /// Note name
        /// </summary>
        public String Name;
        /// <summary>
        /// Midi Note number
        /// </summary>
        public int Number;

        /// <summary>
        /// What octave this note is
        /// </summary>
        public int Octave;

        /// <summary>
        /// Key color (white key or black?)
        /// </summary>
        public KeyColor KeyColor;

        public override string ToString()
        {
            return String.Format("{2}: {0} {1}hz", Name, Frequency, Number);
        }
    }

    public class MidiNotes : ReadOnlyDictionary<int, MidiNote>
    {
        private MidiNotes(double baseFreq, IDictionary<int, MidiNote> dictionary) : base(dictionary)
        {
            BaseFrequency = baseFreq;
        }

        /// <summary>
        /// Frequency of A4.
        /// </summary>
        public double BaseFrequency { get; private set; }

        public static MidiNotes GenerateNotes(float a4Freq=440.0f)
        {
            var noteDict = new Dictionary<int, MidiNote>();

            for (var i = 0; i < 128; i++)
            {
                var note = new MidiNote(i, a4Freq);
                noteDict[i] = note;
            }

            return new MidiNotes(a4Freq, noteDict);
        }

    }
}
