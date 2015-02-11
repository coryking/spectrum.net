using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Dsp.Harmonics
{
    public abstract class ParameterizedHarmonicPosition : IHarmonicPosition
    {

        public int Parameter1 { get; set; }
        public int Parameter2 { get; set; }
        public int Parameter3 { get; set; }



        protected abstract float getNhr(int n, float n0, float par1, float par2);


        public abstract string Name
        {
            get;
        }

        public float GetPosition(int fundamentalFrequency)
        {
            float par1 = (float)Math.Pow(10.0f, -(1.0f - Parameter1 / 255.0f) * 3.0f);
            float par2 = Parameter2 / 255.0f;

            float n0 = (float)fundamentalFrequency - 1.0f;

            var result = getNhr(fundamentalFrequency, n0, par1, par2);

            float par3 = Parameter3 / 255.0f;

            float iresult = (float)Math.Floor(result + 0.5f);
            float dresult = result - iresult;

            var total =  iresult + (1.0f - par3) * dresult;
            Debug.WriteLine("total: {0:f}, fundamentalFreq: {1}, result: {2:f}", total, fundamentalFrequency, result);
            return total;
        }
    }


    public class LinearHarmonic : ParameterizedHarmonicPosition
    {

        public override string Name
        {
            get { return "Linear"; }
        }


        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            return (float)n;
        }
    }

    public class PowerHarmonicPosition : ParameterizedHarmonicPosition
    {
        public override string Name
        {
            get { return "Power"; }
        }


        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            var tmp = Math.Pow(par2 * 2.0f, 2.0f) + 0.1f;
            return n0 * (float)Math.Pow(1.0f + par1 * Math.Pow(n0 * 0.8f, tmp), tmp) + 1.0f;
        }
    }

    public class HarmonicHarmonicPosition : ParameterizedHarmonicPosition
    {

        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            var thresh = (int)(par2 * par2 * 100.0f) + 1;
            if (n < thresh)
                return n;
            else
                return  1.0f + n0 + (n0 - thresh + 1.0f) * par1 * 8.0f;
        }

        public override string Name
        {
            get { return "Harmonic"; }
        }
    }

    public class ShiftHarmonicPosition : ParameterizedHarmonicPosition
    {

        protected override float getNhr(int n, float n0, float par1, float par2)
        {

            return (n + Parameter1 / 255.0f) / (Parameter1 / 255.0f + 1);


        }

        public override string Name
        {
            get { return "Shift"; }
        }
    }

    public class ShiftUHarmonicPosition : ParameterizedHarmonicPosition
    {

        protected override float getNhr(int n, float n0, float par1, float par2)
        {

            var thresh = (int)(par2 * par2 * 100.0f) + 1;
            if (n < thresh)
                return n;
            else
                return 1.0f + n0 - (n0 - thresh + 1.0f) * par1 * 0.90f;
        }

        public override string Name
        {
            get { return "Shift U"; }
        }
    }


    public class ShiftLHarmonicPosition : ParameterizedHarmonicPosition
    {

        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            var tmp = par1 * 100.0f + 1.0f;
            return (float)Math.Pow(n0 / tmp, 1.0f - par2 * 0.8f) * tmp + 1.0f;
        }

        public override string Name
        {
            get { return "Shift L"; }
        }
    }

    public class PowerLHarmonicPosition : ParameterizedHarmonicPosition
    {

        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            return n0
                     * (1.0f
                        - par1)
                     + (float)Math.Pow(n0 * 0.1f, par2 * 3.0f
                            + 1.0f) * par1 * 10.0f + 1.0f;
        }

        public override string Name
        {
            get { return "Power L"; }
        }
    }

    public class PowerUHarmonicPosition : ParameterizedHarmonicPosition
    {

        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            var tmp = par1 * 100.0f + 1.0f;
            return (float)Math.Pow(n0 / tmp, 1.0f - par2 * 0.8f) * tmp + 1.0f;
        }

        public override string Name
        {
            get { return "PowerU"; }
        }
    }

    public class SineHarmonicPosition : ParameterizedHarmonicPosition
    {
        protected override float getNhr(int n, float n0, float par1, float par2)
        {
            return n0
                     + (float)Math.Sin(n0 * par2 * par2 * Math.PI
                            * 0.999f) * (float)Math.Sqrt(par1) * 2.0f + 1.0f;
        }

        public override string Name
        {
            get { return "Sine"; }
        }
    }


}
