using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    /// <summary>
    /// Change the rate the input source reads at
    /// </summary>
    public class ReadRateChangeProvider : ISampleProvider
    {
        static readonly int PerChannelBufferSize = 1024 * 10;

        CorySignalGenerator.Utils.CircularBuffer m_outputBuffer;
        float[] m_inputBuffer;

        ISampleProvider m_source;
        int m_sourceStride;

        public ReadRateChangeProvider(ISampleProvider source, int sourceStride)
        {
            if (sourceStride < 1)
                throw new ArgumentOutOfRangeException("sourceStride", "Source stride has to be a positive integer");

            m_sourceStride = sourceStride;
            m_source = source;
            m_outputBuffer = new Utils.CircularBuffer(source.WaveFormat.Channels * PerChannelBufferSize);
            m_inputBuffer = new float[sourceStride];
        }
        public int Read(float[] buffer, int offset, int count)
        {
            // For now we only support having strides smaller than count
            bool isGood = count >= m_sourceStride && m_source != null;
            Debug.Assert(isGood);
            if (!isGood)
                return 0;

            var readFrames = ReadIntoOutputBuffer();
            while(readFrames == m_sourceStride && m_outputBuffer.Count <= count)
            {
                readFrames = ReadIntoOutputBuffer();
            }

            var framesToWrite = (int)Math.Min(m_outputBuffer.Count, count);
            var framesWritten = m_outputBuffer.Read(buffer, offset, framesToWrite);

            return framesWritten;
        }

        /// <summary>
        /// Read some stuff from the input, write it to the output buffer
        /// </summary>
        /// <returns></returns>
        private int ReadIntoOutputBuffer()
        {
            var readFrames = m_source.Read(m_inputBuffer, 0, m_sourceStride);
            m_outputBuffer.Write(m_inputBuffer, 0, readFrames);
            return readFrames;
        }

        public WaveFormat WaveFormat
        {
            get { return m_source.WaveFormat; }
        }
    }
}
