using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FFTWSharp;
using System.Runtime.InteropServices;

using CorySignalGenerator.Extensions;

using MoreLinq;
using System.Diagnostics;
using NAudio.Dsp;

namespace CorySignalGenerator.Reverb
{
    public class NativeFFTFrame : IFFTFrame
    {
        private int fftSize;

        private IntPtr fft_forward_plan;
        private IntPtr fft_inverse_plan;
        protected IntPtr fft_buffer;

        public float[] RealData
        {
            get;
            private set;
        }

        public Complex[] ComplexData { get;private set; }

        public NativeFFTFrame(int fftSize)
        {
            this.fftSize=fftSize;
            fft_buffer = fftwf.malloc(fftSize * 8);
            fft_forward_plan = fftwf.r2r_1d(fftSize, fft_buffer, fft_buffer, fftw_kind.R2HC, fftw_flags.Estimate);
            fft_inverse_plan = fftwf.r2r_1d(fftSize, fft_buffer, fft_buffer, fftw_kind.HC2R, fftw_flags.Estimate);
            RealData = new float[fftSize];
        }

        ~NativeFFTFrame()
        {
            fftwf.free(fft_buffer);
        }


        public void DoFFT(float[] data)
        {
            Marshal.Copy(data, 0, fft_buffer, fftSize);
            fftwf.execute(fft_forward_plan);
            Marshal.Copy(fft_buffer, RealData, 0, fftSize);

        }

        public void DoInverseFFT(float[] m_outputBuffer, int offset = 0)
        {
            bool isGood = (m_outputBuffer != null && m_outputBuffer.Length >= fftSize);
            Debug.Assert(isGood);
            if (!isGood)
                return;
            Marshal.Copy(RealData, 0, fft_buffer, fftSize);
            fftwf.execute(fft_inverse_plan);
            Marshal.Copy(fft_buffer, m_outputBuffer, 0, fftSize);
            m_outputBuffer.Scale(1f / fftSize); // fftw scales everything by fftSize
        }

        public void DoPaddedFFT(float[] data, int offset, int dataSize)
        {
            float[] buffer = data.Skip(offset).Take(dataSize).Pad(fftSize, 0).ToArray();
            DoFFT(buffer);
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
            if (kernel is NativeFFTFrame)
                this.RealData.MultiplyHalfComplex(kernel.RealData);
            this.DoInverseFFT(output, outputOffset);

        }

        public override string ToString()
        {
            return String.Format("NativeFFTFrame: (len: {0})", fftSize);
        }

    }
}
