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
    public class FFTFrame
    {
        private int fftSize;
        private int m;

#if USE_FFTW
        private IntPtr fft_forward_plan;
        private IntPtr fft_inverse_plan;
        protected IntPtr fft_buffer;

#endif

        public FFTFrame(int fftSize)
        {
            //MathNet.Numerics.Control.UseNativeMKL();
            // TODO: Complete member initialization
            this.fftSize = fftSize;
            this.m = (int)Math.Log(fftSize, 2.0);
#if USE_FFTW
            fft_buffer =fftwf.malloc(fftSize * 8);
            fft_forward_plan = fftwf.r2r_1d(fftSize, fft_buffer, fft_buffer, fftw_kind.R2HC, fftw_flags.Estimate);
            fft_inverse_plan = fftwf.r2r_1d(fftSize, fft_buffer, fft_buffer, fftw_kind.HC2R, fftw_flags.Estimate);
            Data = new float[fftSize];
#endif
        }
        ~FFTFrame()
        {
            fftwf.free(fft_buffer);
        }


#if USE_FFTW
        protected float[] Data
        {
            get;
            private set;
        }
#else
        protected Complex[] Data
        {
            get;
            private set;
        }
#endif
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
#if USE_FFTW
            float[] buffer = data.Skip(offset).Take(dataSize).Pad(fftSize, 0).ToArray();
            DoFFT(buffer);
#else
            Data = data.Skip(offset).Take(dataSize).Pad(fftSize, 0).Select(x => new Complex(){X=x}).ToArray(); //.ToComplex().ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(true, this.m, Data);
#endif
            //MathNet.Numerics.IntegralTransforms.Fourier.Forward(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            //FastFourierTransform.FFT(true, this.m, Data);

        }


        public void DoFFT(float[] data)
        {
#if USE_FFTW
            Marshal.Copy(data, 0, fft_buffer, fftSize);
            fftwf.execute(fft_forward_plan);
            Marshal.Copy(fft_buffer, Data, 0, fftSize);
#else
            Data = data.Select(x => new Complex() { X = x }).ToArray();//new System.Numerics.Complex(x, 0)).ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(true, this.m, Data);
            //MathNet.Numerics.IntegralTransforms.Fourier.Forward(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
//            FastFourierTransform.FFT(true, this.m, Data);
#endif
        }

        public void Multiply(FFTFrame fftKernel)
        {
            bool isGood = (this.Data != null && fftKernel != null && fftKernel.Data != null && this.Data.Length == fftKernel.Data.Length && this.Data.Length == fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;
#if USE_FFTW
            MultiplyHalfComplex(this.Data, fftKernel.Data);
#else
            MultiplyComplex(this.Data, fftKernel.Data);
#endif
            //Data = Data.MultiplyComplexNumbers(fftKernel.Data).ToArray();
        }

        public static unsafe void MultiplyHalfComplex(float[] input, float[] scale)
        {
            if (input.Length != scale.Length)
                throw new ArgumentException("Different FFT sizes!");

            int pos = 0;
            int halfSize = input.Length/2;
            fixed(float* inputRealP = &input[0], scaleRealP = &scale[0])
            {
                float* inputReal = inputRealP;
                float* scaleReal = scaleRealP;
                float* inputComplex = inputReal + halfSize;
                float* scaleComplex = scaleReal + halfSize;
                while (pos < halfSize)
                {
                    float outReal = *inputReal * *scaleReal - *inputComplex * *scaleComplex;
                    float outComplex = *inputReal * *scaleComplex + *inputComplex * *scaleReal;

                    *inputReal = outReal;
                    *inputComplex = outComplex;

                    ++inputComplex; ++scaleComplex;
                    ++inputReal; ++scaleReal;
                    ++pos;
                }
            }
            
        }
        public static void MultiplyComplex(Complex[] inputBuffer, Complex[] scaleBuffer)
        {

            if (inputBuffer.Length != scaleBuffer.Length)
                throw new ArgumentException("Different FFT sizes!");

            for (int i = 0; i < inputBuffer.Length; i++)
            {
                Complex input = inputBuffer[i];
                Complex scale = scaleBuffer[i];
                Complex output;
                output.X = input.X * scale.X - input.Y * scale.Y;
                output.Y = input.X * scale.Y + input.Y * scale.X;
                inputBuffer[i] = output;
                //Data[i] = System.Numerics.Complex.Multiply(Data[i], fftKernel.Data[i]); // output;
            }
        }

        public void DoInverseFFT(float[] m_outputBuffer, int offset=0)
        {
 
            bool isGood = (m_outputBuffer != null && m_outputBuffer.Length >= fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;
#if USE_FFTW
            Marshal.Copy(Data, 0, fft_buffer, fftSize);
            fftwf.execute(fft_inverse_plan);
            Marshal.Copy(fft_buffer, m_outputBuffer, 0, fftSize);
            m_outputBuffer.Scale(1f / fftSize); // fftw scales everything by fftSize
#else
            //MathNet.Numerics.IntegralTransforms.Fourier.Inverse(Data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            FastFourierTransform.FFT(false, this.m, Data);
            Data.ToReal(m_outputBuffer, offset);
#endif
        }

        public override string ToString()
        {
            return String.Format("FFTFrame: (len: {0}, m: {1})", fftSize, m);
        }
    }
}
