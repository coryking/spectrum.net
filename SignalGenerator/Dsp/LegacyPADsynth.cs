﻿using CorySignalGenerator.SampleProviders;
using CorySignalGenerator.Utils;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Dsp
{
    public interface IHarmonicPosition
    {
        string Name { get; }
        float GetPosition(int fundamentalFrequency);
    }


    public class LegacyPADsynth
    {
        private float[] A;
        //private float[] freq_amp;
        private int samplerate;
        private int number_harmonics;
        private System.Random rnd;

        protected int N;

        /// <summary>
        /// Generate a new sample source
        /// </summary>
        /// <param name="amplitudeValues">Array of base amplitude values (@440hz)</param>
        /// <param name="numberHarmonics">Number of harmonics (eg: 10)</param>
        /// <param name="sampleSize">Number of samples (should be a power of two)</param>
        /// <param name="sampleRate">Sample rate (eg: 44100)</param>
        /// <param name="freq">the fundamental frequency (eg. 440 Hz)</param>
        /// <param name="bw">bandwidth in cents of the fundamental frequency (eg. 25 cents)</param>
        /// <param name="bwscale"> how the bandwidth increase on the higher harmonics (recomanded value: 1.0)</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="midiNote">The midi note number</param>
        /// <returns></returns>
        public static SampleSource GenerateWaveTable(float[] amplitudeValues, float freq, float bw, float bwscale, IHarmonicPosition harmonicmaker, int midiNote, int sampleSize, int sampleRate, int channels=1)
        {
            Debug.WriteLine("Building Wave Table\n> freq: {0}. bw: {1}, bwscale: {2}", freq, bw, bwscale);
        
            var synth = new LegacyPADsynth(sampleSize, sampleRate, amplitudeValues, harmonicmaker);
            var sampleData = synth.synth(freq, bw, bwscale);

            var outputData = new float[sampleData.Length * channels];

            var channelOffsets = sampleData.Length / channels;
            for (var x = 0; x < channels; x++)
            {
                for (var i = 0; i < sampleData.Length; i++)
                {
                    var outputPos = i * channels + x;
                    var inputPos = i + x * channelOffsets;
                    inputPos %= sampleData.Length;
                    outputData[outputPos] = sampleData[inputPos];
                }
            }
            return new SampleSource(outputData,freq, midiNote, true, true, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">is the samplesize (eg: 262144)</param>
        /// <param name="samplerate">samplerate (eg. 44100)</param>
        /// <param name="number_harmonics">the number of harmonics that are computed</param>
        public LegacyPADsynth(int n, int samplerate, float[] a, IHarmonicPosition harmonicmaker)
        {
            rnd = new System.Random();
            N=n;
            this.samplerate=samplerate;
            this.number_harmonics=a.Length;
            this.harmonicMaker = harmonicmaker;
            A = a; // new float[number_harmonics];
            //freq_amp = new float[N/2];
        }
        /// <summary>
        /// set the amplitude of the n'th harmonic
        /// </summary>
        /// <param name="n"></param>
        /// <param name="value"></param>
        public void setharmonic(int n, float value)
        {
            if (n < 1 || n >= number_harmonics)
                return;
            A[n] = value;
        }
        /// <summary>
        /// get the amplitude of the n'th harmonic
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public float getharmonic(int n)
        {
            if ((n < 1) || n >= number_harmonics)
                return 0.0f;
            return A[n];
        }


        /// <summary>
        /// generates the wavetable
        /// </summary>
        /// <param name="f">the fundamental frequency (eg. 440 Hz)</param>
        /// <param name="bw">bandwidth in cents of the fundamental frequency (eg. 25 cents)</param>
        /// <param name="bwscale"> how the bandwidth increase on the higher harmonics (recomanded value: 1.0)</param>
        /// <returns></returns>
        public float[] synth(float f, float bw, float bwscale)
        {
            var freq_amp = GenerateSpectrum(f, bw, bwscale);

            var complex_freq = new System.Numerics.Complex[N]; // new Complex[N / 2];
            for (var i = 0; i < N/2; i++)
            {
                var phase = RND() * 2.0f * Math.PI;
                complex_freq[i] = new System.Numerics.Complex(freq_amp[i] * Math.Cos(phase), freq_amp[i] * Math.Sin(phase));
                //complex_freq[i].X = Convert.ToSingle(freq_amp[i] * Math.Cos(phase));
                //complex_freq[i].Y = Convert.ToSingle(freq_amp[i] * Math.Sin(phase));

            }
            //var m = (int)Math.Log(N/2, 2.0);
            MathNet.Numerics.IntegralTransforms.Fourier.Radix2Inverse(complex_freq, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            //FastFourierTransform.FFT(false, m, complex_freq);
            var max = 0d;
            for (var i = 0; i < N; i++)
            {
                if (Math.Abs(complex_freq[i].Real) > max)
                {
                    max = Math.Abs(complex_freq[i].Real);
                }

            }
            if (max < 1e-5)
                max = 1e-5f;
            var output = new float[N];
            for (var i = 0; i < N; i++)
            {
                output[i] = (float)(complex_freq[i].Real / (max * 1.4142d));
            }
            return output;
        }

        /// <summary>
        /// Generates the long spectrum for Bandwidth mode (only amplitudes are generated; phases will be random)
        /// </summary>
        /// <param name="f"></param>
        /// <param name="bw"></param>
        /// <param name="bwscale"></param>
        /// <returns></returns>
        private float[] GenerateSpectrum(float f, float bw, float bwscale)
        {
            var freq_amp = new float[N / 2];
            for (var nh = 1; nh < number_harmonics; nh++)
            {
                var realfreq = f * relF(nh);
                if (realfreq > samplerate * 0.499999f || realfreq < 20.0f)
                {
                    Debug.WriteLine("Bailing out of synth. f: {0:f}, realfreq: {1:f}, nh: {2}", f, realfreq, nh);
                    break;
                }

                var bw_Hz = (Math.Pow(2.0, bw / 1200.0) - 1.0) * f * Math.Pow(realfreq / f, bwscale);
                var bwi = bw_Hz / (2.0 * samplerate);
                var fi = realfreq / samplerate;
#if SHOW_DEBUG
                Debug.WriteLine("freq: {0}, bw_Hz, {1}, bwi: {2}, fi: {3}, bwscale: {4}, bw: {5}, relF({6}): {7}", f, bw_Hz, bwi, fi, bwscale, bw, nh, relF(nh));
#endif
                for (var i = 0; i < freq_amp.Length; i++)
                {
                    var fiH = ((double)i / (double)N) - fi;
                    var hprofile = profile(fiH, bwi);
#if SHOW_DEBUG
                    Debug.WriteLineIf((i < 2), String.Format("> i: {0}, fiH: {1}", i, fiH));
                    Debug.WriteLineIf((hprofile > 0.0), String.Format("> i: {0} fiH: {1}, hProfile: {2}", i, fiH, hprofile));
#endif
                    freq_amp[i] += Convert.ToSingle(hprofile * A[nh]);
                }
            }
            return freq_amp;
        }


        /// <summary>
        /// This method returns the N'th overtone's position relative to the fundamental frequency. By default it returns N. You may override it to make metallic sounds or other instruments where the overtones are not harmonic.
        /// </summary>
        /// <param name="nh"></param>
        /// <returns></returns>
        protected virtual double relF(int nh)
        {
            return harmonicMaker.GetPosition(nh);
        }
        /// <summary>
        /// This is the profile of one harmonic
	    ///In this case is a Gaussian distribution (e^(-x^2))
	    ///The amplitude is divided by the bandwidth to ensure that the harmonic
	    ///keeps the same amplitude regardless of the bandwidth
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="bwi"></param>
        /// <returns></returns>
        protected virtual double profile(double fi, double bwi)
        {
            var x = fi / bwi;
            x *= x;
            //Debug.WriteLine("> x: {0}", x);
            if (x > 14.71280603) 
                return 0.0d;//this avoids computing the e^(-x^2) where it's results are very close to zero
            return Math.Exp(-x) / bwi;// FastMath.FastExp(-x) / bwi; //Math.Exp(-x) / bwi;
        }
        /// <summary>
        /// a random number generator that returns values between 0 and 1
        /// </summary>
        /// <returns></returns>
        protected virtual float RND()
        {
            return (float)rnd.NextDouble();
        }

        protected IHarmonicPosition harmonicMaker { get; set; }
    }
}
