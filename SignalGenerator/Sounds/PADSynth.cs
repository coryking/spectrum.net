using CorySignalGenerator.Dsp;
using CorySignalGenerator.Models;
using CorySignalGenerator.Oscillator;
using CorySignalGenerator.SampleProviders;
using CorySignalGenerator.Sequencer;
using CorySignalGenerator.Utils;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using System.Diagnostics;
using CorySignalGenerator.Filters;
using CorySignalGenerator.Sounds.PAD;

namespace CorySignalGenerator.Sounds
{

    public enum BandwidthScale
    {
        Normal = 0,
        EqualHz = 1,
        Quarter = 2,
        Half = 3,
        ThreeQuarters = 4,
        OneFifty = 5,
        Double = 6,
        InvHalf = 7,
    }

    public static class BandwidthScaleExtensions
    {
        /// <summary>
        /// Translate the <see cref="BandwidthScale"/> into a float.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static float GetScale(this BandwidthScale scale)
        {
            float bwscale = 0f;
            switch (scale)
            {
                    
                case BandwidthScale.Normal:
                    bwscale=1.0f;
                    break;
                case BandwidthScale.EqualHz:
                    bwscale=0f;
                    break;
                case BandwidthScale.Quarter:
                    bwscale = 0.25f;
                    break;
                case BandwidthScale.Half:
                    bwscale = 0.5f;
                    break;
                case BandwidthScale.ThreeQuarters:
                    bwscale = 0.75f;
                    break;
                case BandwidthScale.OneFifty:
                    bwscale = 1.5f;
                    break;
                case BandwidthScale.Double:
                    bwscale = 2.0f;
                    break;
                case BandwidthScale.InvHalf:
                    bwscale = -0.5f;
                    break;
                default:
                    bwscale = 1.0f;
                    break;
            }
            return bwscale;
        }
    }

    public class PADSynth : NoteSampler
    {
        private Random _random;
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

        public PADSynth(WaveFormat format)
        {
            _random = new Random();
            Profile = new PAD.HarmonicProfile();
            WaveFormat = format;
            WaveTable = new ConcurrentDictionary<MidiNote, SampleSource>();
            BuildWaveTableCommand = new RelayCommand(BuildWaveTableCommandExecute);
            SampleSize = (int)Math.Pow(2, 16);
        }

        protected override NAudio.Wave.ISampleProvider GenerateNote(Models.MidiNote note)
        {
            if(WaveTable.Count == 0)
            {
                return new MusicSampleProvider(SampleSource.CreateEmpty(WaveFormat, note));
            }
            
            // This should be logarithmic, not linear
            var nearestNote = WaveTable.Keys.MinBy(x => Math.Abs(x.Frequency - note.Frequency));
            SampleSource source=null;
            var gotSample = WaveTable.TryGetValue(nearestNote, out source);

            // This should always get something... we just looked it up.  Nothing ever deletes samples either...
            Debug.Assert(gotSample);
            if (!gotSample)
                return null; // honestly returning an empty SampleProvider would be a more graceful way to fail

            var music_sampler = new MusicSampleProvider(source);
            ISampleProvider outputProvider;
            var noteDelta = note.Number - nearestNote.Number;
            if (noteDelta != 0)
            {
                var windowFactor = 4 + 4 * note.Number / 128;
                var windowSize = 50f; //windowFactor * 1000 * 1 / nearestNote.FundamentalFrequency;
                var overlapSize = windowSize * 2 / 5f;
                Debug.WriteLine("Shift {0} to {1}. w: {2}, o: {3}", note, nearestNote, windowSize, overlapSize);

                outputProvider = new SuperPitch(music_sampler)
                {
                    PitchOctaves = 0f,
                    PitchSemitones = noteDelta,
                    WindowSize = windowSize,
                    OverlapSize = overlapSize
                };
            }
            else
            {
                outputProvider = music_sampler;
            }
            return outputProvider;
        }

        protected SampleSource GenerateSampleForFrequency(MidiNote note, float bwadjust, float[] profile)
        {
            var spectrum = GenerateSpectrumBandwidth((float)note.Frequency, profile, bwadjust);

            // Do a big-ass FFT...  just one
            MathNet.Numerics.IntegralTransforms.Fourier.Radix2Inverse(spectrum, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);

            var sample = spectrum.Select(x => (float)x.Real).ToArray();
            FrequencyUtils.RmsNormalize(sample, SampleSize);

            return new SampleSource(sample, (float)note.Frequency, note.Number, true, true, WaveFormat);

        }

        public Complex[] GenerateSpectrumBandwidth(float basefreq, float[] profile, float bwadjust)
        {
            var oscilsize = OscillatorGenerator.OSCILLATOR_SIZE;
            var spectrum = new float[SampleSize];

            var harmonics = Oscillator.GetFrequencies(basefreq);
            FrequencyUtils.NormalizeMax(harmonics, oscilsize / 2);

            var power = BandwidthScale.GetScale(); // This is not right but for now we will role with it
            var bandwidthcents = getRealBandwidth();
            for (int nh = 1; nh < oscilsize / 2; nh++)
            {
                var realfreq = HarmonicOffsetMaker.GetPosition(nh) * basefreq;
                if (realfreq > WaveFormat.SampleRate * 0.49999f)
                    break;
                if (realfreq < 20.0f)
                    break;
                if (harmonics[nh - 1] < 1e-4)
                    continue;
                //compute the bandwidth of each harmonic
                var bw =
                    ((FloatMath.pow(2.0f, bandwidthcents / 1200.0f) - 1.0f) * basefreq / bwadjust)
                    * FloatMath.pow(realfreq / basefreq, power);
                var ibw = (int)((bw / (WaveFormat.SampleRate * 0.5f) * SampleSize)) + 1;

                float amp = harmonics[nh - 1];

                // TODO resonance
                if (ibw > profile.Length)
                {
                    var rap = FloatMath.sqrt((float)profile.Length / (float)ibw);
                    var cfreq = (int)(realfreq / (WaveFormat.SampleRate * 0.5f) * SampleSize) - ibw / 2;
                    for (int i = 0; i < ibw; i++)
                    {
                        var src = Convert.ToInt32(FloatMath.floor(i * rap * rap));
                        var spfreq = i + cfreq;
                        if (spfreq < 0)
                            continue;
                        if (spfreq >= SampleSize)
                            break;

                        spectrum[spfreq] += amp + profile[src] * rap;
                    }
                }
                else
                {
                    var rap = FloatMath.sqrt((float)ibw / (float)profile.Length);
                    var ibasefreq = realfreq / (WaveFormat.SampleRate * 0.5f) * SampleSize;
                    for (int i = 0; i < profile.Length; i++)
                    {
                        var idfreq = (i / (float)profile.Length - 0.5f) * ibw;
                        var spfreq = (int)(idfreq + ibasefreq);
                        var fspfreq = FloatMath.mod((float)idfreq + ibasefreq, 1.0f);
                        if (spfreq <= 0)
                            continue;
                        if (spfreq >= SampleSize - 1)
                            break;
                        spectrum[spfreq] += amp * profile[i] * rap * (1.0f - fspfreq);
                        spectrum[spfreq + 1] += amp * profile[i] * rap * fspfreq;
                    }
                }

            }
            return spectrum.Select(x =>
            {
                return Complex.FromPolarCoordinates(x, _random.NextDouble() * 2.0 * Math.PI);
            }).ToArray();

        }

        protected void BuildWaveTable()
        {
            var profile = new float[HarmonicProfile.PROFILE_SIZE];
            var bwadjust = Profile.GetHarmonicProfile(profile);

            var allNotes = MidiNotes.GenerateNotes();
            var notesToGen = notesToSample.Select(x => allNotes[x]);
            
            Debug.WriteLine("Min Freq: {0}, Max Freq: {1}", notesToGen.Min(x => x.Frequency), notesToGen.Max(x => x.Frequency));

            Parallel.ForEach(notesToGen, (note) =>
            {
                var sample = GenerateSampleForFrequency(note, bwadjust, profile);
                WaveTable.AddOrUpdate(note, sample, (k,v) => { return sample; });
            });
        }
        #region Relay Commands
        public RelayCommand BuildWaveTableCommand { get; set; }

        /// <summary>
        /// Method that gets run when somebody executes the <see cref="BuildWaveTableCommand"/>
        /// </summary>
        /// <param name="parameter"></param>
        protected void BuildWaveTableCommandExecute(object parameter)
        {
            Task.Run(() =>
            {
                BuildWaveTable();
            });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Wave lookup table.  Each key is a midi note number
        /// </summary>
        protected ConcurrentDictionary<MidiNote, SampleSource> WaveTable
        {
            get;
            private set;
        }

        private Lazy<OscillatorGenerator> _oscillator = new Lazy<OscillatorGenerator>(() => new OscillatorGenerator());
        public OscillatorGenerator Oscillator { get { return _oscillator.Value; } }

        #region Property HarmonicOffsetMaker
        private IHarmonicPosition _harmonicOffsetMaker = null;

        /// <summary>
        /// Sets and gets the HarmonicOffsetMaker property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IHarmonicPosition HarmonicOffsetMaker
        {
            get
            {
                return _harmonicOffsetMaker;
            }
            set
            {
                Set(ref _harmonicOffsetMaker, value);
            }
        }
        #endregion


        public PAD.HarmonicProfile Profile { get; private set; }

        /// <summary>
        /// bandwidth in cents of the fundamental frequency (eg. 25 cents)
        /// </summary>
        public float Bandwidth { get; set; }

        /// <summary>
        ///  Compute the real bandwidth in cents and returns it
        /// </summary>
        /// <returns></returns>
        private float getRealBandwidth()
        {
            var result = FloatMath.pow(Bandwidth / 1000.0f, 1.1f);
            result = FloatMath.pow(10.0f, result * 4.0f) * 0.25f;
            return result;
        }


        /// <summary>
        /// how the bandwidth increase on the higher harmonics (recomanded value: 1.0)
        /// </summary>
        public BandwidthScale BandwidthScale { get; set; }

        /// <summary>
        /// Gets the size of the sample (must be a power of two)
        /// </summary>
        public int SampleSize { get; set; }


        public override NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            protected set;
        }


        protected override bool SupportsVelocity
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "PAD Synth"; }
        }

        #endregion

    }
}
