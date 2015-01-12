using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Dsp;

namespace UnitTests
{
    [TestClass]
    public class HalfComplexTests
    {
        [TestMethod]
        public void TestComplex()
        {
            var input = new Complex[] { new Complex() { X = 1, Y = 10 }, new Complex() { X = 1, Y = 10 } };
            var scale = new Complex[] { new Complex() { X = 10, Y = 1 }, new Complex() { X = 10, Y = 1 } };
            var expected = new Complex[] { new Complex() { X = 0, Y = 101 }, new Complex() { X = 0, Y = 101 } };
            CorySignalGenerator.Reverb.FFTFrame.MultiplyComplex(input, scale);
            CollectionAssert.AreEqual(expected, input);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var input = new float[] { 1, 1, 10, 10 };
            var scale = new float[] { 10, 10, 1, 1 };
            var expected = new float[] { 0, 0, 101, 101 };

            CorySignalGenerator.Reverb.FFTFrame.MultiplyHalfComplex(input, scale);
            CollectionAssert.AreEqual(expected, input);

        }
    }
}
