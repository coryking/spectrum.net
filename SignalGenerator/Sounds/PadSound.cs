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
            Debug.WriteLineIf((noteDelta != 0), String.Format("Gonna have to pitch shift note {0} ({1}hz) to {2}", noteNumber, frequency, nearestNote));
            if(noteNumber != 0)
            {
                outputProvider = new SuperPitch(music_sampler)
                {
                    PitchOctaves=0f,
                    PitchSemitones=noteDelta,
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
            for (var i = 9; i < allNotes.Keys.Count; i += 12)
            {
                notesToGen.Add(allNotes[i]);
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
