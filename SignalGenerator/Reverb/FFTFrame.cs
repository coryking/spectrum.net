using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using CorySignalGenerator.Extensions;
using System.Diagnostics;
using MathNet.Numerics;

namespace CorySignalGenerator.Reverb
{
    public class FFTFrame
    {
        private int fftSize;
        private int m;
        public FFTFrame(int fftSize)
        {
            //MathNet.Numerics.Control.UseNativeMKL();
            // TODO: Complete member initialization
            this.fftSize = fftSize;
            this.m = (int)Math.Log(fftSize, 2.0);
        }

        public Complex[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Do the reverb stuff...
        /// </summary>
        /// <param name="kernel">The FFT kernel to convolve with</param>
        /// <param name="sample">The sample</param>
        /// <param name="output">An output buffer</param>
        /// <param name="outputOffset">Offset into output buffer</param>
        public void FFTConvolve(FFTFrame kernel, float[] sample, float[] output, int outputOffset)
        {
            this.DoFFT(sample);
            this.Multiply(kernel);
            this.DoInverseFFT(output,outputOffset);
        }

        public void DoPaddedFFT(IEnumerable<float> data, int offset, int dataSize)
        {
            // Pad our sample until we hit FFTSize
            Data = data.Skip(offset).Take(dataSize).Pad(fftSize, 0).Select(x => new Complex(){X=x}).ToArray(); //.ToComplex().ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(true, this.m, Data);
            //MathNet.Numerics.IntegralTransforms.Fourier.Forward(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            //FastFourierTransform.FFT(true, this.m, Data);

        }


        public void DoFFT(IEnumerable<float> data)
        {
            //Data = data.ToComplex().ToArray();
            Data = data.Select(x => new Complex() { X = x }).ToArray();//new System.Numerics.Complex(x, 0)).ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(true, this.m, Data);
            //MathNet.Numerics.IntegralTransforms.Fourier.Forward(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
//            FastFourierTransform.FFT(true, this.m, Data);

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
                //Data[i] = System.Numerics.Complex.Multiply(Data[i], fftKernel.Data[i]); // output;
            }
            //Data = Data.MultiplyComplexNumbers(fftKernel.Data).ToArray();
        }

        public void DoInverseFFT(float[] m_outputBuffer, int offset=0)
        {
            bool isGood = (m_outputBuffer != null && m_outputBuffer.Length >= fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;
            //MathNet.Numerics.IntegralTransforms.Fourier.Inverse(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            FastFourierTransform.FFT(false, this.m, Data);
            Data.ToReal(m_outputBuffer, offset);
        }

        public override string ToString()
        {
            return String.Format("FFTFrame: (len: {0}, m: {1})", fftSize, m);
        }
    }
}
