using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using CorySignalGenerator.Extensions;
using System.Diagnostics;
using MathNet.Numerics;

#if USE_FFTW
using FFTWSharp;
using System.Runtime.InteropServices;
#endif

namespace CorySignalGenerator.Reverb
{
    public class FFTFrame : IFFTFrame
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


        public float[] RealData
        {
            get;
            private set;
        }
        public Complex[] ComplexData
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
        public void FFTConvolve(IFFTFrame kernel, float[] sample, float[] output, int outputOffset)
        {

            this.DoFFT(sample);
            if(kernel is FFTFrame)
                this.ComplexData.MultiplyComplex(kernel.ComplexData);
            
            this.DoInverseFFT(output,outputOffset);
        }

        public void DoPaddedFFT(float[] data, int offset, int dataSize)
        {
            // Pad our sample until we hit FFTSize
            float[] buffer = data.Skip(offset).Take(dataSize).Pad(fftSize, 0).ToArray();
            DoFFT(buffer);
            //MathNet.Numerics.IntegralTransforms.Fourier.Forward(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            //FastFourierTransform.FFT(true, this.m, Data);

        }


        public void DoFFT(float[] data)
        {

            ComplexData = data.Select(x => new Complex() { X = x }).ToArray();//new System.Numerics.Complex(x, 0)).ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(true, this.m, ComplexData);
            //MathNet.Numerics.IntegralTransforms.Fourier.Forward(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
//            FastFourierTransform.FFT(true, this.m, Data);
        }


        
        public void DoInverseFFT(float[] m_outputBuffer, int offset=0)
        {
 
            bool isGood = (m_outputBuffer != null && m_outputBuffer.Length >= fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;

            //MathNet.Numerics.IntegralTransforms.Fourier.Inverse(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            FastFourierTransform.FFT(false, this.m, ComplexData);
            ComplexData.ToReal(m_outputBuffer, offset);
        }

        public override string ToString()
        {
            return String.Format("FFTFrame: (len: {0}, m: {1})", fftSize, m);
        }
    }
}
