using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Reverb.Fakes;
using CorySignalGenerator.Reverb;
using Microsoft.QualityTools.Testing.Fakes;
namespace UnitTests
{
    [TestClass]
    public class FFTConvolverTests :BaseShimTests
    {
       

      
        [TestMethod]
        public void TestFFTConvolver_HalfSize()
        {
            var kernel_size = 4;
            var sample_size = 2;

            var output_buffer_1 = new float[sample_size];
            var output_buffer_2 = new float[sample_size];
            var output_buffer_3 = new float[sample_size];
            var convolver = new FFTConvolver(kernel_size);
            var sample_data_1 = new float[] { 1, 2 };
            var sample_data_2 = new float[] { 3, 4 };
            var sample_data_3 = new float[] { 5, 6 };
            var kernel_data = GetData(kernel_size, (i) => { return i; });

            ShimFFTFrame.AllInstances.FFTConvolveFFTFrameSingleArraySingleArrayInt32 = (t, k, source, output, offset) =>
                {
                    Array.Copy(source, output, source.Length);
                };

            var fftFrame = new ShimFFTFrame();
            convolver.Process(fftFrame, sample_data_1, 0, output_buffer_1, 0, sample_size);
            convolver.Process(fftFrame, sample_data_2, 0, output_buffer_2, 0, sample_size);
            convolver.Process(fftFrame, sample_data_3, 0, output_buffer_3, 0, sample_size);

            // first should be zeros
            CollectionAssert.AreEqual(new float[] { 0, 0 }, output_buffer_1);
            // now we should start seeing the output
            CollectionAssert.AreEqual(sample_data_1, output_buffer_2);
            CollectionAssert.AreEqual(sample_data_2, output_buffer_3);

        }

        [TestMethod]
        public void TestFFTConvolver_FullSize()
        {
            var kernel_size = 4;
            var sample_size = 4;

            var output_buffer_1 = new float[sample_size];
            var output_buffer_2 = new float[sample_size];
            var output_buffer_3 = new float[sample_size];
            var convolver = new FFTConvolver(kernel_size);
            var sample_data_1 = new float[] { 1, 2, 3, 4 };
            var sample_data_2 = new float[] { 5, 6, 7, 8 };
            var kernel_data = GetData(kernel_size, (i) => { return i; });

            ShimFFTFrame.AllInstances.FFTConvolveFFTFrameSingleArraySingleArrayInt32 = (t, k, source, output, offset) =>
                {
                    Array.Copy(source, output, source.Length);
                };

            var fftFrame = new ShimFFTFrame();
            convolver.Process(fftFrame, sample_data_1, 0, output_buffer_1, 0, sample_size);
            convolver.Process(fftFrame, sample_data_2, 0, output_buffer_2, 0, sample_size);

            // first half should be zeros
            CollectionAssert.AreEqual(new float[] { 0, 0, 1, 2 }, output_buffer_1);
            // now we should see more stuff...
            CollectionAssert.AreEqual(new float[] { 3, 4, 5, 6}, output_buffer_2);
        }
    }
}
