using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using CorySignalGenerator.SampleProviders;

namespace UnitTests
{
    public class DummyProvider : ISampleProvider
    {
        Func<float[], int, int, int> _reader;
        WaveFormat _format;
        public DummyProvider(Func<float[], int,int, int> reader, WaveFormat format=null)
        {
            if (format == null)
                _format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            else
                _format = format;

            _reader = reader;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _reader.Invoke(buffer, offset, count);
        }

        public WaveFormat WaveFormat
        {
            get { return _format; }
        }
    }

    [TestClass]
    public class ReadRateChangeTests
    {
        [TestMethod]
        public void TestReadRateChanger()
        {
            var sourceSample = new float[][] {
            new float[]{ 1,2,3,4},
            new float[]{ 5,6,7,8},
            };
            var expectedDestBuffer = new float[] { 1, 2, 3, 4, 5,6,7,8 };
            var destBuffer = new float[8];
            var sourceStride = 4;
            var destStride = 8;

            ReadRateTest(sourceSample, expectedDestBuffer, destBuffer,8, sourceStride, destStride);

        }
        

        [TestMethod]
        public void TestReadRate_UnevenBytes()
        {
            // Make sure we can handle when the input sample stops (by returning less than the requested number of bytes)

            var sourceSample = new float[][] {
            new float[]{ 1,2,3,4},
            new float[]{ 5,},
            };
            var expectedDestBuffer = new float[] { 1, 2, 3, 4, 5,0,0,0 };
            var destBuffer = new float[8];
            var sourceStride = 4;
            var destStride = 8;

            ReadRateTest(sourceSample, expectedDestBuffer, destBuffer, 5, sourceStride, destStride);

        }

        private static void ReadRateTest(float[][] sourceSample, float[] expectedDestBuffer, float[] destBuffer, int expectedFramesRead, int sourceStride, int destStride)
        {
            var callCount = 0;
            var source = new DummyProvider((buffer, offset, count) =>
            {
                if (callCount >= sourceSample.Length)
                    return 0;

                var sample = sourceSample[callCount];
                Assert.AreEqual(sourceStride, count);
                Assert.AreEqual(0, offset);
                Array.Copy(sample, buffer, sample.Length);
                callCount++;
                return sample.Length;
            });

            // The read function should have been called once ber element in sourceSample
            var provider = new ReadRateChangeProvider(source, sourceStride);

            var framesRead = provider.Read(destBuffer, 0, destStride);
            Assert.AreEqual(sourceSample.Length, callCount);

            Assert.AreEqual(expectedFramesRead, framesRead);
            CollectionAssert.AreEqual(expectedDestBuffer, destBuffer);
        }

    }
}
