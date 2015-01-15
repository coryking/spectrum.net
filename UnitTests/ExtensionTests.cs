using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Extensions;
namespace UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var floats = new float[] { 1, 2, 3, 4 };
            var result = floats.TakeChannel(0,floats.Length);

            CollectionAssert.AreEqual(new float[] { 1, 3 }, result);

            result = floats.TakeChannel(0, 1);
            CollectionAssert.AreEqual(new float[] { 1 }, result);

        }

        [TestMethod]
        public void TestWithChannel2()
        {
            var floats = new float[] { 1, 2, 3, 4 };
            var result = floats.TakeChannel(1, floats.Length);

            CollectionAssert.AreEqual(new float[] { 2, 4 }, result);

            result = floats.TakeChannel(1, 1);
            CollectionAssert.AreEqual(new float[] { 2 }, result);


        }
        
        [TestMethod]
        public void TestInterleave()
        {
            var items = new float[] { 2, 2 };
            var output = new float[] { 0, 0, 0, 0 };
            items.InterleaveChannel(output, 0, 0,items.Length );
            CollectionAssert.AreEqual(new float[] { 2, 0, 2, 0 }, output);

            var output2 = new float[] { 0, 0, 0, 0 };
            items.InterleaveChannel(output2, 1,0,items.Length);
            CollectionAssert.AreEqual(new float[] { 0, 2, 0, 2 }, output2);

            Array.Clear(output2,0,output2.Length);
            items.InterleaveChannel(output2, 0, 0, 1);
            CollectionAssert.AreEqual(new float[] { 2, 0, 0, 0 }, output2);;

            Array.Clear(output2, 0, output2.Length);
            items.InterleaveChannel(output2, 0, 1, 1);
            CollectionAssert.AreEqual(new float[] { 0, 2, 0, 0 }, output2); ;
        }

        [TestMethod]
        public void TestSumOfSquares()
        {
            var items = new float[] { 1, 2, 3 };
            var expected = 1 + 2 * 2 + 3 * 3;

            var actual = items.SumOfSquares(3, 0, channels: 1);
            Assert.AreEqual(expected, actual);

            expected = 1 + 3 * 3;
            actual = items.SumOfSquares(2, 0);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestScale()
        {
            var items = new float[] { 2, 4, 6 };
            items.Scale(0.5f);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, items);
        }
    }
}
