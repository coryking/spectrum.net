using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Utils
{
    public static class FrequencyUtils
    {
        public static float ScaleFrequency(float baseFrequency, float shift, float range)
        {
            double exponent = shift / range;
            return baseFrequency * (float)Math.Pow(2.0d, exponent);

        }

        public static double Normal(this Complex complex)
        {
            return Math.Pow(complex.Magnitude, 2.0);
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
            for (int i = 0; i < length; i++)
            {
                sum += frequencies[i].Normal(); 
            }
            if (sum < 0.000001)
                return; // everything is just about zero.  Nothing to do...

            var gain = 1.0 / Math.Sqrt(sum);
            for (int i = 0; i < length; i++)
            {
                frequencies[i] *= gain;
            }
        }
    }
}
