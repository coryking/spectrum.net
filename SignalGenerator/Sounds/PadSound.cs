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

namespace CorySignalGenerator.Sounds
{
    public class PadSound : ISoundModel
    {

        protected ConcurrentDictionary<float, SampleSource> WaveTable
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
            WaveTable = new ConcurrentDictionary<float, SampleSource>();
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            private set;
        }

        public NAudio.Wave.ISampleProvider GetProvider(float frequency, int velocity)
        {
            if (!IsWaveTableLoaded)
                throw new InvalidOperationException("Cannot get a provider.  No wave table has been created");
            var nearestFreq = WaveTable.Keys.MinBy(x => Math.Abs(x - frequency));
            var music_sampler = new MusicSampleProvider(WaveTable[nearestFreq]);
            return new AdsrSampleProvider(music_sampler)
            {
                AttackSeconds=AttackSeconds,
                ReleaseSeconds = ReleaseSeconds
            };
        }


       
        public void InitSamples()
        {
            var freqs = MidiNotes.GenerateNotes().Values.Select(x => (float)x.Frequency);
            Debug.WriteLine("Min Freq: {0}, Max Freq: {1}", freqs.Min(), freqs.Max());
            //var freqs = new float[] { 440f };
            Parallel.ForEach(freqs, (frequency) =>
            {
                var harmonics = Harmonics * 440 / (int)frequency;

                var sample =
                     PADsynth.GenerateWaveTable(frequency, Bandwidth, BandwidthScale, harmonics, SampleSize, WaveFormat.SampleRate, WaveFormat.Channels);
                WaveTable.AddOrUpdate(frequency, sample, (key, value) => sample);

            });
            
            IsWaveTableLoaded = true;
        }
    }
}
