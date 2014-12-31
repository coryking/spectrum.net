using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Dsp
{
    /// <summary>
    ///  Ghetto Linear Interpolation
    /// </summary>
    public class LinearInterpolator
    {
        public static float[] Upsample(float[] original, int factor)
        {
            var newSize = original.Length * factor;
            var output = new float[newSize];
            for (var i = 0; i < original.Length; i++)
            {
                var current = original[i];
                float next = (i < original.Length -1) ? original[i+1] : 0f;
                var delta = next - current;
                for (var f = 0; f < factor; f++)
                {
                    output[i * factor + f] =current + delta * f / factor;
                }
            }

            return output;
        }

        public static float[] Downsample(float[] original, int factor)
        {
            var newSize = (int)Math.Ceiling((double)original.Length / factor);
            var output = new float[newSize];
            for (var i = 0; i < newSize; i ++)
            {
                var origIndex = i * factor;
                output[i] = original[origIndex];
            }
            return output;
        }

        public static float[] Rescale(float[] original, float factor)
        {
            if (factor == 0)
                throw new ArgumentOutOfRangeException("factor", "factor has to be greater than zero");
            if (factor == 1)
                return original;

            if( factor < 1)
                return Downsample(original, Convert.ToInt32( 1 / factor));

            return Upsample(original, Convert.ToInt32(factor));
        }

    }
}
