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
            var result = floats.TakeChannel(0);

            CollectionAssert.AreEqual(new float[] { 1, 3 }, result);

        }

        [TestMethod]
        public void TestWithChannel2()
        {
            var floats = new float[] { 1, 2, 3, 4 };
            var result = floats.TakeChannel(1);

            CollectionAssert.AreEqual(new float[] { 2, 4 }, result);

        }

        [TestMethod]
        public void TestInterleave()
        {
            var items = new float[] { 2, 2 };
            var output = new float[] { 0, 0, 0, 0 };
            items.Interleave(output, 2, 0);
            CollectionAssert.AreEqual(new float[] { 2, 0, 2, 0 }, output);

            var output2 = new float[] { 0, 0, 0, 0 };
            items.Interleave(output2, 2, 1);
            CollectionAssert.AreEqual(new float[] { 0, 2, 0, 2 }, output2);
            
        }
    }
}
