using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Dsp
{
    public class PADsynth
    {
        private float[] A;
        private float[] freq_amp;
        private int samplerate;
        private int number_harmonics;
        private System.Random rnd;
        protected int N;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">is the samplesize (eg: 262144)</param>
        /// <param name="samplerate">samplerate (eg. 44100)</param>
        /// <param name="number_harmonics">the number of harmonics that are computed</param>
        public PADsynth(int n, int samplerate, int number_harmonics)
        {
            rnd = new System.Random();
            N=n;
            this.samplerate=samplerate;
            this.number_harmonics=number_harmonics;

            A = new float[number_harmonics];
            freq_amp = new float[N/2];
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
            Array.Clear(freq_amp,0,freq_amp.Length);
            for (var nh = 1; nh < number_harmonics; nh++)
            {
                var rF = f * relF(nh);

                var bw_Hz = (Math.Pow(2.0, bw / 1200.0) - 1.0) * f * Math.Pow(relF(nh), bwscale);
                var bwi = bw_Hz / (2.0 * samplerate);
                var fi = rF / samplerate;
                for (var i = 0; i < N / 2; i++)
                {
                    var hprofile = profile((i / (double)N) - fi, bwi);
                    freq_amp[i] += Convert.ToSingle(hprofile * A[nh]);
                }
            }

            var complex_freq = new Complex[N / 2];
            for (var i = 0; i < N; i++)
            {
                var phase = RND() * 2.0f * Math.PI;
                complex_freq[i].X = Convert.ToSingle(freq_amp[i] * Math.Cos(phase));
                complex_freq[i].Y = Convert.ToSingle(freq_amp[i] * Math.Sin(phase));

            }
            var m = (int)Math.Log(N/2, 2.0);
            FastFourierTransform.FFT(false, m, complex_freq);
            var max = 0f;
            for (var i = 0; i < N; i++)
            {
                if (Math.Abs(complex_freq[i].X) > max)
                {
                    max = Math.Abs(complex_freq[i].X);
                }

            }
            if (max < 1e-5)
                max = 1e-5f;
            var output = new float[N];
            for (var i = 0; i < N; i++)
            {
                output[i] = complex_freq[i].X / max * 1.4142f;
            }
            return output;
        }


        /// <summary>
        /// This method returns the N'th overtone's position relative to the fundamental frequency. By default it returns N. You may override it to make metallic sounds or other instruments where the overtones are not harmonic.
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        protected virtual float relF(int N)
        {
            return N;
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
            float x = fi / bwi;
            x *= x;
            if (x > 14.71280603) 
                return 0.0f;//this avoids computing the e^(-x^2) where it's results are very close to zero
            return (float)Math.Exp(-x) / bwi;
        }
        /// <summary>
        /// a random number generator that returns values between 0 and 1
        /// </summary>
        /// <returns></returns>
        protected virtual float RND()
        {
            return (float)rnd.NextDouble();
        }
    }
}
