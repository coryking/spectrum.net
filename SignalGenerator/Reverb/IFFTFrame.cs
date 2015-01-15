using NAudio.Dsp;
using System;
namespace CorySignalGenerator.Reverb
{
    public interface IFFTFrame
    {
        void DoFFT(float[] data);
        void DoInverseFFT(float[] m_outputBuffer, int offset = 0);
        void DoPaddedFFT(float[] data, int offset, int dataSize);
        void FFTConvolve(IFFTFrame kernel, float[] sample, float[] output, int outputOffset);

        Complex[] ComplexData { get; }
        float[] RealData { get; }

    }
}
