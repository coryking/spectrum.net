using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class InterpolatorTest
    {
        [TestMethod]
        public void Test2x()
        {
            
            var output = CorySignalGenerator.Dsp.LinearInterpolator.Upsample(new float[]{0f,1f,2f},2);
            var expectedOutput = new float[]{
                0f,
                0.5f,
                1f,
                1.5f,
                2f,
                1f
            };
            CollectionAssert.AreEqual(expectedOutput, output);

        }

        [TestMethod]
        public void Test1x()
        {
            var output = CorySignalGenerator.Dsp.LinearInterpolator.Upsample(new float[]{0f,1f,2f},1);
            var expectedOutput = new float[]{
                0f,
                1f,
                2f,
            };
            CollectionAssert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void TestDownsample1x()
        {
                        var output = CorySignalGenerator.Dsp.LinearInterpolator.Downsample(new float[]{0f,1f,2f},1);
            var expectedOutput = new float[]{
                0f,
                1f,
                2f,
            };
            CollectionAssert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void TestDownsample2x()
        {
            var output = CorySignalGenerator.Dsp.LinearInterpolator.Downsample(new float[] { 0f, 1f, 2f, 3f}, 2);
            var expectedOutput = new float[]{
                0f,
                2f,
            };
            CollectionAssert.AreEqual(expectedOutput, output);
        }
        [TestMethod]
        public void TestDownsample5x()
        {
            var output = CorySignalGenerator.Dsp.LinearInterpolator.Downsample(new float[] { 0f, 1f, 2f, 3f, 4f,5f,6f,7f }, 5);
            var expectedOutput = new float[]{
                0f,
                5f,
            };
            CollectionAssert.AreEqual(expectedOutput, output);
        }


        [TestMethod]
        public void TestDownsample3xWithOdd()
        {
            var output = CorySignalGenerator.Dsp.LinearInterpolator.Downsample(new float[] { 0f, 1f, 2f }, 3);
            var expectedOutput = new float[]{
                0f,
            };
            CollectionAssert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void TestRescale()
        {
            var outputUp = CorySignalGenerator.Dsp.LinearInterpolator.Rescale(new float[] { 0f, 1f }, 2f);
            CollectionAssert.AreEqual(new float[] { 0f, 0.5f, 1f, 0.5f }, outputUp);
            var outputDown = CorySignalGenerator.Dsp.LinearInterpolator.Rescale(new float[] { 0f, 0.5f, 1f, 0.5f }, 0.5f);
            CollectionAssert.AreEqual(new float[] { 0f,1f }, outputDown);
        }
    }
}
