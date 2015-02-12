using CorySignalGenerator.Dsp;
using CorySignalGenerator.Oscillator;
using CorySignalGenerator.SampleProviders;
using CorySignalGenerator.Sequencer;
using CorySignalGenerator.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sounds
{



    public class PADSynth : NoteSampler
    {
        public const int PROFILE_SIZE = 512;
        private Random _random;

        public PADSynth(WaveFormat format)
        {
            _random = new Random();
            HarmonicProfile = new PAD.HarmonicProfile();
            WaveFormat = format;
        }

        protected override NAudio.Wave.ISampleProvider GenerateNote(Models.MidiNote note)
        {
            throw new NotImplementedException();
        }

        protected float GetProfile(float[] profile, int profilesize)
        {
            var supersample = 16;
            var basepar = FloatMath.pow(2.0f, (1.0f - HarmonicProfile.BaseWidth / 127.0f) * 12.0f);
            var freqmult = FloatMath.floor(FloatMath.pow(2.0f, HarmonicProfile.FrequencyMultiplier / 127.0f * 5.0f) + 0.000001f);
            var modfreq = FloatMath.floor(FloatMath.pow(2.0f, HarmonicProfile.ModulatorFrequency / 127.0f * 5.0f) + 0.000001f);
            var modpar1 = FloatMath.pow(HarmonicProfile.BaseWidth / 127.0f, 4.0f) * 5.0f / FloatMath.sqrt(modfreq);

            float amppar1 = FloatMath.pow(2.0f, FloatMath.pow(HarmonicProfile.AmplitudeMultiplierParam1 / 127.0f, 2.0f) * 10.0f) - 0.999f;
            float amppar2 = (1.0f - HarmonicProfile.AmplitudeMultiplierParam2 / 127.0f) * 0.998f + 0.001f;

            var width = FloatMath.pow(150.0f / (HarmonicProfile.Size + 22.0f), 2.0f);

            for (int i = 0; i < profilesize * supersample; i++)
            {
                var makezero = false;
                var x = i * 1.0f / (profilesize * (float)supersample);
                float origx = x;
                //do the sizing (width)
                x = (x - 0.5f) * width + 0.5f;
                if (x < 0.0f)
                {
                    x = 0.0f;
                    makezero = true;
                }
                else if (x > 1.0f)
                {
                    x = 1.0f;
                    makezero = true;
                }
                // if you want to do anything with the profile... do it here
                float x_before_freq_mult = x;
                x *= freqmult;

                //do the modulation of the profile
                x += FloatMath.sin(x_before_freq_mult * 3.1415926f * modfreq) * modpar1;
                x = FloatMath.mod(x + 1000.0f, 1.0f) * 2.0f - 1.0f;

                float f;
                switch (HarmonicProfile.BaseType)
                {
                    case CorySignalGenerator.Sounds.PAD.FrequencyBaseType.Square:
                        f = FloatMath.exp(-(x * x) * basepar);
                        if (f < 0.4f)
                            f = 0.0f;
                        else
                            f = 1.0f;
                        break;
                    case CorySignalGenerator.Sounds.PAD.FrequencyBaseType.DoubleExp:
                        f = FloatMath.exp(-(FloatMath.abs(x)) * FloatMath.sqrt(basepar));

                        break;
                    default: // gaussian
                        f = FloatMath.exp(-(x * x) * basepar);
                        break;
                }

                if (makezero)
                    f = 0.0f;

                float amp = 1.0f;
                origx = origx * 2.0f - 1.0f;

                switch (HarmonicProfile.AplitudeMultiplierType)
                {
                    case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplierType.Gauss:
                        amp = FloatMath.exp(-(origx * origx) * 10.0f * amppar1);
                        break;
                    case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplierType.Sine:
                        amp = 0.5f * (1.0f + FloatMath.cos(3.1415926f * origx * FloatMath.sqrt(amppar1 * 4.0f + 1.0f)));
                        break;
                    case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplierType.Flat:
                        amp = 1.0f / (FloatMath.pow(origx * (amppar1 * 2.0f + 0.8f), 14.0f) + 1.0f);
                        break;
                    default:
                        break;
                }

                float finalsmp = f;
                if(HarmonicProfile.AplitudeMultiplierType != PAD.AmplitudeMultiplierType.OFF)
                {
                    switch (HarmonicProfile.AmplitudeMultiplerMode)
                    {
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Sum:
                            finalsmp = amp * (1.0f - amppar2) + finalsmp * amppar2;

                            break;
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Mult:
                            finalsmp *= amp * (1.0f - amppar2) + amppar2;

                            break;
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Div1:
                            finalsmp = finalsmp / (amp + FloatMath.pow(amppar2, 4.0f) * 20.0f + 0.0001f);
                            break;
                        case CorySignalGenerator.Sounds.PAD.AmplitudeMultiplerMode.Div2:
                            finalsmp = amp / (finalsmp + FloatMath.pow(amppar2, 4.0f) * 20.0f + 0.0001f);
                            break;
                        default:
                            break;
                    }
                }
                profile[i / supersample] += finalsmp / supersample;

            }

            FrequencyUtils.NormalizeToOne(profile, profilesize);

            if (!AutoScale)
                return 0.5f;

            //compute the estimated perceived bandwidth
            float sum = 0.0f;
            int index;
            for (index = 0; index < profilesize / 2 - 2; ++index)
            {
                sum += profile[index] * profile[index] + profile[profilesize - index - 1] * profile[profilesize - index - 1];
                if (sum >= 4.0f)
                    break;
            }

            float result = 1.0f - 2.0f * index / (float)profilesize;
            return result;
        }

        protected SampleSource GenerateSampleForFrequency(float frequency, float bwadjust, int midiNote, float[] profile)
        {
            var spectrum = GenerateSpectrumBandwidth(frequency, profile, bwadjust);

            // Do a big-ass FFT...  just one
            MathNet.Numerics.IntegralTransforms.Fourier.Radix2Inverse(spectrum, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);

            var sample = spectrum.Select(x => (float)x.Real).ToArray();
            FrequencyUtils.RmsNormalize(sample, SampleSize);

            return new SampleSource(sample, frequency, midiNote, true, true, WaveFormat);

        }

        public Complex[] GenerateSpectrumBandwidth(float basefreq, float[] profile, float bwadjust)
        {
            var oscilsize = OscillatorGenerator.OSCILLATOR_SIZE;
            var spectrum = new float[SampleSize];

            var harmonics = Oscillator.GetFrequencies(basefreq);
            FrequencyUtils.NormalizeMax(harmonics, oscilsize / 2);

            var power = BandwidthScale; // This is not right but for now we will role with it
            var bandwidthcents = Bandwidth; // this too is not right... should somehow get converted into cents
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
                        var src = Convert.ToInt32(i * rap * rap);
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
                        spectrum[spfreq] += amp * profile[i] * rap * (1.0f + fspfreq);
                        spectrum[spfreq + 1] += amp * profile[i] * rap * fspfreq;
                    }
                }

            }
            return spectrum.Select(x =>
            {
                return Complex.FromPolarCoordinates(x, _random.NextDouble() * 2.0 * Math.PI);
            }).ToArray();

        }

        #region Properties

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


        public PAD.HarmonicProfile HarmonicProfile { get; private set; }


        #region Property AutoScale
        private bool _autoScale = false;

        /// <summary>
        /// Sets and gets the AutoScale property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AutoScale
        {
            get
            {
                return _autoScale;
            }
            set
            {
                Set(ref _autoScale, value);
            }
        }
        #endregion
		

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
