using CorySignalGenerator.Reverb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class DummyFFTFrame : IFFTFrame
    {

        public void DoFFT(float[] data)
        {
            // do nothing!
        }

        public void DoInverseFFT(float[] m_outputBuffer, int offset = 0)
        {
            // do nothing!
        }

        public void DoPaddedFFT(float[] data, int offset, int dataSize)
        {
            // do nothing!
        }

        public void FFTConvolve(IFFTFrame kernel, float[] sample, float[] output, int outputOffset)
        {
            // also do nothing
            Array.Copy(sample, output, sample.Length);
        }

        public NAudio.Dsp.Complex[] ComplexData
        {
            get { return null; }
        }

        public float[] RealData
        {
            get { return null; }
        }
    }

}
