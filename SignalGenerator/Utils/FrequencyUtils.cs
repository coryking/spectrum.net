using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
