using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Utils;

namespace UnitTests
{
    [TestClass]
    public class CircularBufferTests
    {
        [TestMethod]
        public void TestArrayIndex()
        {
            int size=3;


            CircularBuffer<int> circularBuffer = new CircularBuffer<int>(size);

            for (int n = 0; n < size; n++)
            {
                circularBuffer[n]=n;
            }
            Assert.AreEqual(0, circularBuffer[0]);
            Assert.AreEqual(1, circularBuffer[1]);
            Assert.AreEqual(2, circularBuffer[2]);

            circularBuffer.Advance(1);
            Assert.AreEqual(0, circularBuffer[1]);
            Assert.AreEqual(1, circularBuffer[2]);

            circularBuffer[0]=10;
            Assert.AreEqual(10, circularBuffer[0]);
            Assert.AreEqual(0, circularBuffer[1]);
            Assert.AreEqual(1, circularBuffer[2]);


            circularBuffer.Advance(1);
            Assert.AreEqual(10, circularBuffer[1]);
            Assert.AreEqual(0, circularBuffer[2]);
        }

    }
}
