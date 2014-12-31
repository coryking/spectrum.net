using CorySignalGenerator.Dsp;
using CorySignalGenerator.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using System.Collections.Concurrent;
using CorySignalGenerator.Extensions;
using CorySignalGenerator.Models;
using System.Diagnostics;
using CorySignalGenerator.Filters;

namespace CorySignalGenerator.Sounds
{
    public class PadSound : ISoundModel
    {

        private readonly int[] notesToSample = new int[]{
            9, // octave -1
            21, 23, // octave 0
            24, 25, 27, 31, 35, // octave 1
            36, 38, 40, 41, 43, 45, 47, // octave 2
            48, 50, 52, 53, 55, 57, 59, // octave 3
            60, 62, 64, 65, 67, 69, 71, // octave 4
            72, 74, 76, 77, 79, 81, 83, // octave 5
            84, 86, 88, 89, 91, 93, 95, // octave 6
            96, 98, 101, 105, // octave 7
            108, 116, // octave 8
            120, 127 // octave 9
        };


        /// <summary>
        /// Wave lookup table.  Each key is a midi note number
        /// </summary>
        protected ConcurrentDictionary<int, SampleSource> WaveTable
        {
            get;
            private set;
        }

        protected bool IsWaveTableLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of harmonics (eg: 10)
        /// </summary>
        public int Harmonics { get; set; }

        /// <summary>
        /// bandwidth in cents of the fundamental frequency (eg. 25 cents)
        /// </summary>
        public float Bandwidth { get; set; }

        /// <summary>
        /// how the bandwidth increase on the higher harmonics (recomanded value: 1.0)
        /// </summary>
        public float BandwidthScale { get; set; }

        /// <summary>
        /// Gets the size of the sample (must be a power of two)
        /// </summary>
        public int SampleSize { get; set; }

        public float AttackSeconds { get; set; }
        public float ReleaseSeconds { get; set; }

        public PadSound(WaveFormat waveFormat)
        {
            Bandwidth = 25f;
            BandwidthScale = 1.0f;
            Harmonics = 10;
            WaveFormat = waveFormat;
            SampleSize = waveFormat.SampleRate;
            WaveTable = new ConcurrentDictionary<int, SampleSource>();
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            private set;
        }

        public NAudio.Wave.ISampleProvider GetProvider(float frequency, int velocity, int noteNumber)
        {
            if (!IsWaveTableLoaded)
                throw new InvalidOperationException("Cannot get a provider.  No wave table has been created");
            var nearestNote = WaveTable.Values.MinBy(x => Math.Abs(x.FundamentalFrequency - frequency));
            var music_sampler = new MusicSampleProvider(WaveTable[nearestNote.Note]);
            ISampleProvider outputProvider;
            var noteDelta = noteNumber - nearestNote.Note;
            if(noteDelta != 0)
            {

                var windowSize = 50; // 6 * 1000 * 1 / nearestNote.FundamentalFrequency;
                var overlapSize = 20; // windowSize * 2 / 5f;
                Debug.WriteLine("Shift {0} ({1}hz) to {2}. w: {3}, o: {4}", noteNumber, frequency, nearestNote, windowSize, overlapSize);

                outputProvider = new SuperPitch(music_sampler)
                {
                    PitchOctaves=0f,
                    PitchSemitones=noteDelta,
                    WindowSize=windowSize,
                    OverlapSize = overlapSize
                };
            }
            else
            {
                outputProvider = music_sampler;
            }
            return new AdsrSampleProvider(outputProvider)
            {
                AttackSeconds=AttackSeconds,
                ReleaseSeconds = ReleaseSeconds
            };
        }


       
        public void InitSamples()
        {
            var allNotes = MidiNotes.GenerateNotes();
            var notesToGen = new List<MidiNote>();
            for (var i = 0; i < notesToSample.Length; i++)
            {
                var notenumber = notesToSample[i];
                notesToGen.Add(allNotes[notenumber]);
            }

            Debug.WriteLine("Min Freq: {0}, Max Freq: {1}", notesToGen.Min(x=>x.Frequency), notesToGen.Max(x=>x.Frequency));
            //var freqs = new float[] { 440f };
            Parallel.ForEach(notesToGen, (note) =>
            {
                var harmonics = Harmonics * 440 / (int)note.Frequency;

                var sample =
                     PADsynth.GenerateWaveTable((float)note.Frequency, Bandwidth, BandwidthScale, harmonics, note.Number, SampleSize, WaveFormat.SampleRate, WaveFormat.Channels);
                WaveTable.AddOrUpdate(note.Number, sample, (key, value) => sample);

            });
            
            IsWaveTableLoaded = true;
        }
    }
}
