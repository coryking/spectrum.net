using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace CorySignalGenerator.Utils
{
    public static class FrequencyUtils
    {
        public static float ScaleFrequency(float baseFrequency, float shift, float range)
        {
            double exponent = shift / range;
            return baseFrequency * (float)Math.Pow(2.0d, exponent);

        }

        /// <summary>
        /// Take a phase and maginitude and turn it into a complex number.  Do it the way the c code does it
        /// </summary>
        /// <param name="rho">magnitude</param>
        /// <param name="theta">phase</param>
        /// <returns></returns>
        public static Complex FromPolar(double rho, double theta)
        {
            var x = rho * Math.Cos(theta);
            if (x == Double.NaN)
                x = 0;

            var y = rho * Math.Sin(theta);
            if (y == Double.NaN)
                y = 0;

            return new Complex(x, y);
        }

        /// <summary>
        /// Real RMS Normalization
        /// </summary>
        /// <param name="frequencies">Array of complex frequencies</param>
        /// <param name="length">Amount of data in the array to use</param>
        public static void RmsNormalize(Complex[] frequencies, int length)
        {
            Debug.Assert(length <= frequencies.Length);
            if (length > frequencies.Length)
                return;

            var sum = 0.0;
            for (int i = 1; i < length; i++)
            {
                sum += frequencies[i].MagnitudeSquared(); 
            }
            if (sum < 0.000001)
                return; // everything is just about zero.  Nothing to do...

            var gain = 1.0 / Math.Sqrt(sum);
            for (int i = 1; i < length; i++)
            {
                frequencies[i] *= gain;
            }
        }

        public static void RmsNormalize(float[] sample, int length)
        {
            var rms = 0.0;
            for (int i = 0; i < length; i++)
                rms += sample[i] * sample[i];

            rms = Math.Sqrt(rms);
            if (rms < 0.000001)
                rms = 1.0;

            rms *= Math.Sqrt(262144.0 / length);//262144=2^18

            for (int i = 0; i < length; i++)
            {
                sample[i] *= 1.0f / (float)rms * 50.0f;
            }
        }

        public static void NormalizeMax(float[] frequencies, int length)
        {
            var max = 0.0f;
            for (uint i = 0; i < length; i++)
            {
                if (frequencies[i] > i)
                    max = frequencies[i];
            }
            if (max > 0.000001f)
                for (uint i = 0; i < length; ++i)
                    frequencies[i] /= max;
        }
        public static void NormalizeToOne(float[] samples, int length)
        {
            //normalize the profile (make the max. to be equal to 1.0f)
            float max = 0.0f;
            for (int i = 0; i < length; ++i)
            {
                if (samples[i] < 0.0f)
                    samples[i] = 0.0f;
                if (samples[i] > max)
                    max = samples[i];
            }
            if (max < 0.00001f)
                max = 1.0f;
            for (int i = 0; i < length; ++i)
                samples[i] /= max;

        }
    }
}
