using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Utils;
namespace UnitTests
{
    [TestClass]
    public class VectorMathTests
    {
        [TestMethod]
        public void VectorCopy()
        {
            var buffer = new float[] { 1, 2, 3, 4};

            var dest = new float[2];
            VectorMath.vcopy(buffer, 0, 2, dest, 0, 1, 2);
            CollectionAssert.AreEqual(new float[] {1,3}, dest);
        }

        [TestMethod]
        public void VectorCopy_StrideCheck()
        {
            // make sure we don't plow over stuff in the output we aren't supposed to
            var buffer = new float[] { 1, 2, 3, 4 };

            var dest = new float[] { 10, 10, 10, 10 };
            VectorMath.vcopy(buffer, 0, 2, dest, 0, 2, 2);
            CollectionAssert.AreEqual(new float[] { 1,10, 3, 10 }, dest);
        }

        [TestMethod]
        public void VectorAddTest()
        {
            var buffer = new float[] { 1, 2, 3 };
            var buffer2 = new float[] { 1, 2, 3 };
            var dest = new float[3];
            VectorMath.vadd(buffer, 0, 1, buffer2, 0, 1, dest, 0, 1, 3);
            CollectionAssert.AreEqual(new float[] { 2, 4, 6 }, dest);

        }

        [TestMethod]
        public void VectorAddTest_4x()
        {
            var buffer = new float[] { 1, 2, 3 };
            var buffer2 = new float[] { 1, 2, 3 };
            var buffer3 = new float[] { 1, 2, 3 };
            var buffer4 = new float[] { 1, 2, 3 };
            var dest = new float[3];
            VectorMath.vadd(buffer, 0, 1, buffer2, 0, 1, buffer3, 0, 1, buffer4, 0,1, dest, 0, 1, 3);
            CollectionAssert.AreEqual(new float[] { 4, 8, 12 }, dest);

        }


        [TestMethod]
        public void VectorAddTestWithStride()
        {
            var buffer = new float[] { 1, 10, 2, 10, 3, 10 };
            var buffer2 = new float[] { 1, 1, 1 };
            var dest = new float[3];

            VectorMath.vadd(buffer, 0, 2, buffer2, 0, 1, dest, 0, 1, 3);
            CollectionAssert.AreEqual(new float[] { 2, 3, 4 }, dest);

            
        }

        [TestMethod]
        public void WriteIntoOwnBuffer()
        {
            // should be able to handle writing into its own buffer
            var buffer = new float[] { 1, 2, 3 };
            var buffer2 = new float[] { 1, 2, 3 };
            VectorMath.vadd(buffer, 0, 1, buffer2, 0, 1, buffer, 0, 1, 3);
            CollectionAssert.AreEqual(new float[] { 2, 4, 6 }, buffer);
            
        }
    }
}
