using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{

    /// <summary>
    /// Taken from NAudio WPF demo.
    /// </summary>
    public class SampleSource
    {
#if !NETFX_CORE // only for .NET (NAudio for Win8 doesn't have WaveFileReader)
        public static SampleSource CreateFromWaveFile(string fileName)
        {
            using (var reader = new WaveFileReader(fileName))
            {
                var sp = reader.ToSampleProvider();
                var sourceSamples = (int)(reader.Length / (reader.WaveFormat.BitsPerSample / 8));
                var sampleData = new float[sourceSamples];
                int n = sp.Read(sampleData, 0, sourceSamples);
                if (n != sourceSamples)
                {
                    throw new InvalidOperationException(String.Format("Couldn't read the whole sample, expected {0} samples, got {1}", n, sourceSamples));
                }
                var ss = new SampleSource(sampleData, false, false, sp.WaveFormat);
                return ss;
            }
        }
#endif
        public SampleSource(float[] sampleData,bool isLoopable, bool isRandomStart, WaveFormat waveFormat) :
            this(sampleData, isLoopable, isRandomStart, waveFormat, 0, sampleData.Length)
        {
        }

        public SampleSource(float[] sampleData, bool isLoopable, bool isRandomStart, WaveFormat waveFormat, int startIndex, int length)
        {
            this.SampleData = sampleData;
            this.SampleWaveFormat = waveFormat;
            this.StartIndex = startIndex;
            this.Length = length;
            this.IsLoopable = isLoopable;
            this.IsRandomStart = isRandomStart;
        }

        /// <summary>
        /// Sample data
        /// </summary>
        public float[] SampleData { get; private set; }
        /// <summary>
        /// Format of sampleData
        /// </summary>
        public WaveFormat SampleWaveFormat { get; private set; }
        /// <summary>
        /// Index of the first sample to play
        /// </summary>
        public int StartIndex { get; private set; }
        /// <summary>
        /// Number of valid samples
        /// </summary>
        public int Length { get; private set; }

        public bool IsLoopable { get; private set; }

        public bool IsRandomStart { get; private set; }
    }
}
