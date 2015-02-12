using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Utils
{
    public static class FloatMath
    {

        public static float min(float a, float b) { return Math.Min(a, b); }
        public static float max(float a, float b) { return Math.Max(a, b); }
        public static float abs(float a) { return Math.Abs(a); }
        public static float exp(float a) { return (float)Math.Exp(a); }
        public static float sqrt(float a) { return (float)Math.Sqrt(a); }
        public static float sin(float a) { return (float)Math.Sin(a); }
        public static float tan(float a) { return (float)Math.Tan(a); }
        public static float cos(float a) { return (float)Math.Cos(a); }
        public static float pow(float a, float b) { return (float)Math.Pow(a, b); }
        public static float sign(float a) { return Math.Sign(a); }
        public static float log(float a) { return (float)Math.Log(a); }
        public static float PI { get { return (float)Math.PI; } }
        public static float mod(float a, float b) { return a % b; }

        public static float floor(float a) { return (float)Math.Floor(a); }
    }
}
