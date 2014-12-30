using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    /// <summary>
    /// Taken from NAudio WPF demo
    /// </summary>
    public class MusicSampleProvider : ISampleProvider
    {
        private int SampleOffset;
        private readonly SampleSource sampleSource;
        const int SINGLE_BYTES = 4; // four bytes per float
        private bool isFinished = false; // will be true only when non-loopable

        public MusicSampleProvider(SampleSource sampleSource)
        {
            this.sampleSource = sampleSource;
        }

        public WaveFormat WaveFormat
        {
            get { return this.sampleSource.SampleWaveFormat; }
        }

        protected float[] SampleBuffer
        {
            get { return sampleSource.SampleData; }
        }

        public bool IsLoopable { get { return sampleSource.IsLoopable; } }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isFinished)
                return 0;
            var dstOffset = 0;
            while (dstOffset < count)
            {
                var copyLength = Math.Min(this.SampleBuffer.Length - SampleOffset, count - dstOffset);
                Buffer.BlockCopy(this.SampleBuffer, SINGLE_BYTES * this.SampleOffset, buffer, SINGLE_BYTES * (dstOffset + offset), copyLength * SINGLE_BYTES);
                dstOffset += copyLength;
                SampleOffset += copyLength;
                if (SampleOffset >= this.SampleBuffer.Length)
                {
                    SampleOffset = 0;
                    if (!IsLoopable)
                    {
                        isFinished = true;
                        return dstOffset;
                    }
                }
                        
            }
            return dstOffset;
        }

       
    }
}