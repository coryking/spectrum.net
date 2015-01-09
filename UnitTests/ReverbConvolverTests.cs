using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Reverb;
using CorySignalGenerator.Reverb.Fakes;
namespace UnitTests
{
    [TestClass]
    public class ReverbConvolverTests :BaseShimTests
    {
        [TestMethod]
        public void TestReverbConvolver()
        {
            var impulseResponseSize = 256;
            var renderSliceSize = 64;

            ShimFFTFrame.AllInstances.FFTConvolveFFTFrameSingleArraySingleArrayInt32 = (t, k, source, output, offset) =>
                {
                    Array.Copy(source,0, output, offset, source.Length);
                };

            ShimDirectConvolver.AllInstances.ProcessSingleArraySingleArrayInt32SingleArrayInt32Int32 = (t, kernel, source, o1, dest, o2, count) =>
            {
                Array.Copy(source, o1, dest, o2, count);
            };

            var impulseResponse = new float[impulseResponseSize];
            var convolver = new ReverbConvolver(impulseResponse, renderSliceSize, 32768, 0, false);

            var inputBuffer_1 = GetData(renderSliceSize, (i) => { return 1; });
            var outputBuffer_1 = new float[renderSliceSize];
            convolver.Process(inputBuffer_1, 0, outputBuffer_1, 0, renderSliceSize);


            var inputBuffer_2 = GetData(renderSliceSize, (i) => { return 2; });
            var outputBuffer_2 = new float[renderSliceSize];
            convolver.Process(inputBuffer_2, 0, outputBuffer_2, 0, renderSliceSize);

            CollectionAssert.AreEqual(inputBuffer_1, outputBuffer_1);
            CollectionAssert.AreEqual(inputBuffer_2, outputBuffer_2);

        }
    }
}
