using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.SampleProviders;
using NAudio.Wave;
namespace UnitTests
{
    [TestClass]
    public class MusicSampleProviderTests
    {
        [TestMethod]
        public void TestProperties()
        {
            var sampleFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            var sampleSource = new SampleSource(new float[1], 440, 23, true, false, sampleFormat);

            var msp = new MusicSampleProvider(sampleSource);
            Assert.AreEqual(msp.WaveFormat, sampleFormat);
            Assert.IsTrue(msp.IsLoopable);

            
        }

        [TestMethod]
        public void NonLoopable()
        {
            var msp = GetMusicSampleProvider(4, loopable: false);

            var expectedData = new float[] { 1, 2, 3 };
            var actualData = new float[3];
            msp.Read(actualData, 0, 3);
            CollectionAssert.AreEqual(expectedData, actualData);

            var expectedData2 = new float[] { 4,0,0 };
            var actualData2 = new float[3];
            var samplesRead = msp.Read(actualData2, 0, 3);
            Assert.AreEqual(1, samplesRead);
            CollectionAssert.AreEqual(expectedData2, actualData2);

            var expectedData3 = new float[3];
            var actualData3 = new float[3];
            var samplesRead3 = msp.Read(actualData3, 0, 3);
            Assert.AreEqual(0, samplesRead3);
            CollectionAssert.AreEqual(expectedData3, actualData3);
        }

        [TestMethod]
        public void RandomStart()
        {
            var msp = GetMusicSampleProvider(4, randomStart: true);

            var expectedData = new float[1];
            expectedData[0] = msp.SampleOffset + 1;
            
            var actualData = new float[1];

            var samplesRead = msp.Read(actualData, 0, 1);
            Assert.AreEqual(1, samplesRead);

            CollectionAssert.AreEqual(expectedData, actualData);

        }

        [TestMethod]
        public void TestMusicSampleProvider_bigger_buffer()
        {
            var msp = GetMusicSampleProvider(5);


            var expectedData = new float[] { 1, 2, 3, 4, 5, 1, 2, 3, 4 };
            var actualData = new float[9];

            var samplesRead = msp.Read(actualData, 0, 9);

            Assert.AreEqual(9, samplesRead);
            CollectionAssert.AreEqual(expectedData, actualData);

            // The second call should start with the five and then wrap around
            var expectedData2 = new float[] { 5, 1, 2, 3, 4, 5, 1, 2, 3 };
            var actualData2 = new float[9];

            var samplesRead2 = msp.Read(actualData2, 0, 9);

            Assert.AreEqual(9, samplesRead2);
            CollectionAssert.AreEqual(expectedData2, actualData2);
        }

        

        [TestMethod]
        public void SmallerBuffer()
        {
            var msp = GetMusicSampleProvider(5);

            var expectedData = new float[] { 1, 2, 3 };
            var actualData = new float[3];
            var samplesRead = msp.Read(actualData, 0, 3);

            Assert.AreEqual(3, samplesRead);
            CollectionAssert.AreEqual(expectedData, actualData);

            // test the wraparound
            var expectedData2 = new float[] {4,5,1 };
            var actualData2 = new float[3];

            var samplesRead2 = msp.Read(actualData2, 0, 3);

            Assert.AreEqual(3, samplesRead2);
            CollectionAssert.AreEqual(expectedData2, actualData2);
            
        }

        [TestMethod]
        public void EqualSizedBuffer()
        {
            var msp = GetMusicSampleProvider(4);

            var expectedData = new float[] { 1, 2, 3,4 };
            var actualData = new float[4];
            var samplesRead = msp.Read(actualData, 0, 4);

            Assert.AreEqual(4, samplesRead);
            CollectionAssert.AreEqual(expectedData, actualData);

            // test the wraparound
            var expectedData2 = new float[] { 1,2,3,4 };
            var actualData2 = new float[4];

            var samplesRead2 = msp.Read(actualData2, 0, 4);

            Assert.AreEqual(4, samplesRead2);
            CollectionAssert.AreEqual(expectedData2, actualData2);
            
        }

        private static MusicSampleProvider GetMusicSampleProvider(int count, bool randomStart=false, bool loopable=true)
        {
            var sampleFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            var sampleData = new float[count];
            for (int i = 0; i < count; i++)
            {
                sampleData[i] = i + 1;
            }
            var sampleSource = new SampleSource(sampleData, 440, 23, loopable, randomStart, sampleFormat);

            var msp = new MusicSampleProvider(sampleSource);
            return msp;
        }
    }
}
