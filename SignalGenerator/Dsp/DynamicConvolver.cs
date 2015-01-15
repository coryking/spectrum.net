using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Dsp
{
    public class DynamicConvolver
    {
        private const int L=200; // length of impulse response
        private const int STEPS = 258; // number of amplitude regions
        private const int DV = STEPS - 2;

        private BasicCircularBuffer<double> x = new BasicCircularBuffer<double>(L);
        private BasicCircularBuffer<int> S = new BasicCircularBuffer<int>(L);
        //private double[] x = new double[L];
        //private int[] S = new int[L];

        private double[,] h = new double[STEPS,L];

        /// <summary>
        /// Creates a new instance of the Dynamic Convolver
        /// </summary>
        /// <param name="sampleRate">The sample rate</param>
        /// <param name="cfr">Resonance Frequency</param>
        /// <param name="dp">Frequency Sweep / Non-Linearity Amount</param>
        public DynamicConvolver(int sampleRate, double cfr, double dp)
        {
            double sc = 6.0 / L;
            double frq = Math.PI * 2 * cfr / sampleRate;
            for (var k = 0; k < STEPS;k++)
            {
                double sum = 0;
                double theta = 0;
                double w;
                for (var i = 0; i < L; i++)
                {
                    h[k, i] = Math.Sin(theta) * Math.Exp(-sc * i);
                    w = (double)i / L;
                    theta += frq * (1 + dp * w * (k - 0.4 * STEPS) / STEPS);
                    sum += Math.Abs(h[k, i]);
                }
                double norm = 1.0 / sum;
                for (var i = 0; i < L; i++)
                    h[k, i] *= norm;
            }
        }

        protected unsafe double Conv(int d)
        {
            double y = 0;
            for (var i = 0; i < L; i++)
            {
                var k = S[i] + d;
                y += x[i] * h[k, i];
            }
            return y;
        }

        public unsafe float Operator(float input)
        {
            double A = Math.Abs(input);
            double a, b, w, y = 0;
            int sel=Convert.ToInt32(DV*A);
            
            //for (var j = L - 1; j > 0; j--)
            //{
            //    x[j] = x[j - 1];
            //    S[j] = S[j - 1];
            //}
            x.Advance(1);
            S.Advance(1);
            x[0] = input;
            S[0] = sel;

            if (sel == 0){
                y = Conv(0);
            }
            else if (sel > 0)
            {
                a = Conv(0);
                b = Conv(1);
                w = DV * A - sel;
                y = w * a + (1 - w) * b;
            }
            return Convert.ToSingle(y);

        }
    }
}
