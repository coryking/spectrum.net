using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using CorySignalGenerator.Extensions;
using System.Diagnostics;

namespace CorySignalGenerator.Reverb
{
    public class FFTFrame
    {
        private int fftSize;
        private int m;
        public FFTFrame(int fftSize)
        {
            // TODO: Complete member initialization
            this.fftSize = fftSize;
            this.m = (int)Math.Log(fftSize, 2.0);
        }

        public Complex[] Data
        {
            get;
            private set;
        }

        public void DoPaddedFFT(IEnumerable<float> data, int offset, int dataSize)
        {
            // Pad our sample until we hit FFTSize
            Data = data.Skip(offset).Take(dataSize).Pad(fftSize, 0).ToComplex().ToArray();
            FastFourierTransform.FFT(true, this.m, Data);

        }


        public void DoFFT(IEnumerable<float> data)
        {
            Data = data.ToComplex().ToArray();
            FastFourierTransform.FFT(true, this.m, Data);

        }

        public void Multiply(FFTFrame fftKernel)
        {
            bool isGood = (this.Data != null && fftKernel != null && fftKernel.Data != null && this.Data.Length == fftKernel.Data.Length && this.Data.Length == fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;

            for (int i = 0; i < fftSize; i++)
            {
                Complex input = Data[i];
                Complex scale = fftKernel.Data[i];
                Complex output;
                output.X = input.X * scale.X - input.Y * scale.Y;
                output.Y = input.X * scale.Y + input.Y * scale.X;
                Data[i] = output;
            }
            Data = Data.MultiplyComplexNumbers(fftKernel.Data).ToArray();
        }

        public void DoInverseFFT(float[] m_outputBuffer)
        {
            bool isGood = (m_outputBuffer != null && m_outputBuffer.Length >= fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;

            FastFourierTransform.FFT(false, this.m, Data);
            Data.ToReal(m_outputBuffer, 0);
        }

        public override string ToString()
        {
            return String.Format("FFTFrame: (len: {0}, m: {1})", fftSize, m);
        }
    }
}
