using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Oscillator
{
    public class PowerBaseFunction :IBaseFunction
    {
        public float Process(float x, float a)
        {
            x = FloatMath.mod(x, 1);
            if (a < 0.00001f)
                a = 0.00001f;
            else
                if (a > 0.99999f)
                    a = 0.99999f;
            return FloatMath.pow(x, FloatMath.exp((a - 0.5f) * 10.0f)) * 2.0f - 1.0f;
        }

        public string Name
        {
            get { return "Power"; }
        }
    }

    public class GaussBaseFunction : IBaseFunction
    {
        public float Process(float x, float a)
        {
            x = FloatMath.mod(x, 1) * 2.0f - 1.0f;
            if (a < 0.00001f)
                a = 0.00001f;
            return FloatMath.exp(-x * x * (FloatMath.exp(a * 8) + 5.0f)) * 2.0f - 1.0f;

        }

        public string Name
        {
            get { return "Gaussian"; }
        }
    }

    public class DiodeBaseFunction : IBaseFunction
    {

        public float Process(float x, float a)
        {
            if (a < 0.00001f)
                a = 0.00001f;
            else
                if (a > 0.99999f)
                    a = 0.99999f;
            a = a * 2.0f - 1.0f;
            x = FloatMath.cos((x + 0.5f) * 2.0f * FloatMath.PI) - a;
            if (x < 0.0f)
                x = 0.0f;
            return x / (1.0f - a) * 2 - 1.0f;
        }

        public string Name
        {
            get { return "Diode"; }
        }
    }

    public class SineBaseFunction :IBaseFunction
    {
        public float Process(float x, float a)
        {
            // doesn't do anything.... don't call this.
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return "Sine"; }
        }
    }

}
