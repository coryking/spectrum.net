using CorySignalGenerator.Dsp;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public  class FFTConvolverTests
    {
        [TestMethod]
        public void TestFFT()
        {
            using (ShimsContext.Create())
            {
                CorySignalGenerator.Dsp.Fakes.ShimFFTConvolverHelper.ConvolveFramesSingleArraySingleArrayInt32ComplexArray = (input, output, m, k) =>
                {

                    Array.Copy(input, output, input.Length);
                };
                CorySignalGenerator.Dsp.Fakes.ShimFFTConvolverHelper.FFTTransformBooleanInt32ComplexArray = (f, m, d) =>
                {
                    // do nothing.
                };
                var convolver = new FFTConvolver(4);

                convolver.InitKernel(new float[] { 1, 1, 1, 1 });

                var data = new float[4] { 1, 2, 3, 4 };
                var result = new float[2];
                var count = convolver.Process(data, 0, result, 0, 4);
                Assert.AreEqual(2, count);
                CollectionAssert.AreEqual(new float[] { 1, 2 }, result);

                convolver.Process(new float[4] { 5,6,7,8 }, 0, result, 0, 4);
                CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result);

            }

        }
    }
}
