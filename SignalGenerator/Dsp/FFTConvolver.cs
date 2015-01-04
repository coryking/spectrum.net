using NAudio.Dsp;
using System;
using System.Collections.Generic;
using MoreLinq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
using NAudio.Wave;

namespace CorySignalGenerator.Dsp
{
    public class FFTConvolver
    {
        // Buffer input until we get fftSize / 2 samples then do an FFT
        int m_readWriteIndex;
        float[] m_inputBuffer;

        // Stores output which we read a little at a time
        float[] m_outputBuffer;
        // Saves the 2nd half of the FFT buffer, so we can do an overlap-add with the 1st half of the next one
        float[] m_lastOverlapBuffer;
        Complex[] m_convolutionKernel;

        int m_fftSize;
        int m_M;

        public FFTConvolver()
        {

        }

        public FFTConvolver(int fftSize):this()
        {
            InitBuffers(fftSize);

        }

        private void InitBuffers(int fftSize)
        {
            m_fftSize = fftSize;

            if (!IsPowerOfTwo(fftSize))
                throw new ArgumentOutOfRangeException("power", "sampes has to be a power of two!");

            m_inputBuffer = new float[fftSize];
            m_outputBuffer = new float[fftSize];
            m_lastOverlapBuffer = new float[fftSize / 2];
            m_convolutionKernel = new Complex[fftSize];
            m_M = (int)Math.Log(fftSize, 2.0);
        }

        public int FFTSize { get { return m_fftSize; } }

        public int Process(float[] sourceP, int sourceOffset, float[] destP, int destOffset, int framesToProcess)
        {
            int copied = 0;
            int halfSize = m_fftSize / 2;
            // framesToProcess must be an exact multiple of halfSize,
            // or halfSize is a multiple of framesToProcess when halfSize > framesToProcess.
            var isGood = !((halfSize % framesToProcess==0) && (framesToProcess % halfSize ==0));
            if (!isGood)
                return copied;

            int numberOfDivisions = halfSize <= framesToProcess ? (framesToProcess / halfSize) : 1;
            int divisionSize = numberOfDivisions == 1 ? framesToProcess : halfSize;

            for(var i =0; i<numberOfDivisions; i++)
            {
                Array.Copy(sourceP, sourceOffset, m_inputBuffer, m_readWriteIndex, divisionSize);
                Array.Copy(m_outputBuffer, m_readWriteIndex, destP, destOffset, divisionSize);
                copied += divisionSize;
                m_readWriteIndex += divisionSize;

                // Check if it's time to perform the next FFT
                if (m_readWriteIndex == halfSize)
                {
                    // The input buffer is now filled (get frequency-domain version)
                    var frame = m_inputBuffer.ToComplexArray();
                    FastFourierTransform.FFT(true, m_M, frame);
                    var multiplied = frame.MultiplyComplexNumbers(m_convolutionKernel).ToArray();
                    FastFourierTransform.FFT(false, m_M, multiplied);
                    multiplied.ToReal(m_outputBuffer);

                    m_lastOverlapBuffer.AddWithScale(m_outputBuffer, halfSize);
                    Array.Copy(m_outputBuffer, halfSize, m_lastOverlapBuffer, 0, halfSize);
                    m_readWriteIndex = 0;
                }
            }
            return copied;
        }
        public void Reset()
        {
            m_readWriteIndex = 0;
            Array.Clear(m_lastOverlapBuffer, 0, m_lastOverlapBuffer.Length);
        }

        public void InitKernel(float[] samples)
        {
            m_convolutionKernel = samples.ToComplexArray();
            FastFourierTransform.FFT(true, m_M, m_convolutionKernel);
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public static FFTConvolver InitFromWaveStream(WaveStream waveStream, int channel)
        {
            var bufferSize = Convert.ToInt32(waveStream.Length  / waveStream.WaveFormat.BlockAlign);
            bufferSize = bufferSize % 2 == 0 ? bufferSize : bufferSize + 1;
            var buffer = new float[bufferSize];
            var sampleProvider = waveStream.ToSampleProvider();
            var samplesRead = sampleProvider.Read(buffer, 0, bufferSize);

            var fftBuffer = buffer.TakeChannel(channel, bufferSize / waveStream.WaveFormat.Channels, channels:waveStream.WaveFormat.Channels);

            var fftConvolver = new FFTConvolver(fftBuffer.Length);
            fftConvolver.InitKernel(fftBuffer);
            return fftConvolver;

        }

    }
}
