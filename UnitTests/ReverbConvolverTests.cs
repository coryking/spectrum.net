using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Reverb;
using CorySignalGenerator.Reverb.Fakes;
using System.Collections.Generic;
namespace UnitTests
{
    public struct SampleBlock
    {
        public float[] input_buffer;
        public float[] output_buffer;
        int blockNumber;

        public SampleBlock(int size, int blockNumber)
        {
            input_buffer = new float[size];
            output_buffer = new float[size];
            this.blockNumber = blockNumber;
        }
        public override string ToString()
        {
            if(input_buffer.Length >0)
                return String.Format("SampleBlock {0}.  Len: {1}, i0: {2} o0: {3}", this.blockNumber, input_buffer.Length, input_buffer[0], output_buffer[0]);
            else
                return String.Format("SampleBlock {0}.", this.blockNumber);
        }

    }

    [Ignore] // ignore for now... this gets to crazy to test
    [TestClass]
    public class ReverbConvolverTests :BaseShimTests
    {
        [TestMethod]
        public void TestReverbConvolver()
        {
            var impulseResponseSize = 256;
            var renderSliceSize = 64;

            ShimDirectConvolver.AllInstances.ProcessSingleArraySingleArrayInt32SingleArrayInt32Int32 = (t, kernel, source, o1, dest, o2, count) =>
            {
                Array.Copy(source, o1, dest, o2, count);
            };

            var impulseResponse = new float[impulseResponseSize];
            var convolver = new ReverbConvolver(impulseResponse, renderSliceSize, 32768, 0, false);
            Assert.IsFalse(convolver.HasBackgroundFrames);

            var sampleBlockCount = impulseResponseSize / renderSliceSize;
            var sampleBlocks = new List<SampleBlock>();
            for (int i = 0; i < sampleBlockCount; i++)
            {
                var sampleBlock = new SampleBlock(renderSliceSize,i);
                sampleBlock.input_buffer = GetData(renderSliceSize, (j) => { return i; });
                convolver.Process(sampleBlock.input_buffer, sampleBlock.output_buffer, renderSliceSize);
                sampleBlocks.Add(sampleBlock);
            }

            foreach (var sampleBlock in sampleBlocks)
            {
                CollectionAssert.AreEqual(sampleBlock.input_buffer, sampleBlock.output_buffer, String.Format("Collections are not equal for {0}", sampleBlock)); ;
            }
            
        }

        [Ignore] // ignore for now... this gets to crazy to test
        [TestMethod]
        public void TestReverbConvolver_WithInterestingKernel()
        {
            var impulseResponseSize = 256;
            var renderSliceSize = 64;

            var impulseResponse = new float[impulseResponseSize];
            impulseResponse[0] = 1; // there is no reverb
            var convolver = new ReverbConvolver(impulseResponse, renderSliceSize, 32768, 0, false);
            Assert.IsFalse(convolver.HasBackgroundFrames);

            var sampleBlockCount = impulseResponseSize / renderSliceSize;
            var sampleBlocks = new List<SampleBlock>();
            for (int i = 0; i < sampleBlockCount; i++)
            {
                var sampleBlock = new SampleBlock(renderSliceSize,i);
                sampleBlock.input_buffer = GetData(renderSliceSize, (j) => { return i; });
                convolver.Process(sampleBlock.input_buffer, sampleBlock.output_buffer, renderSliceSize);
                sampleBlocks.Add(sampleBlock);
            }

            foreach (var sampleBlock in sampleBlocks)
            {
                CollectionAssert.AreEqual(sampleBlock.input_buffer, sampleBlock.output_buffer, String.Format("Collections are not equal for {0}", sampleBlock)); ;
            }

        }

        [Ignore] // ignore for now... this gets to crazy to test
        [TestMethod]
        public void TestReverbConvolver_WithVeryLargeKernel()
        {
            var impulseResponseSize = ReverbConvolver.RealtimeFrameLimit + 8192;
            var renderSliceSize = 64;

            var impulseResponse = new float[impulseResponseSize];
            impulseResponse[0] = 1; // there is no reverb
            var convolver = new ReverbConvolver(impulseResponse, renderSliceSize, 32768, 0, true);

            Assert.IsTrue(convolver.HasBackgroundFrames);

            var sampleBlockCount = impulseResponseSize / renderSliceSize;
            var sampleBlocks = new List<SampleBlock>();
            for (int i = 0; i < sampleBlockCount; i++)
            {
                var sampleBlock = new SampleBlock(renderSliceSize, i);
                sampleBlock.input_buffer = GetData(renderSliceSize, (j) => { return i; });
                convolver.Process(sampleBlock.input_buffer, sampleBlock.output_buffer, renderSliceSize);
                sampleBlocks.Add(sampleBlock);
            }



            foreach (var sampleBlock in sampleBlocks)
            {
                CollectionAssert.AreEqual(sampleBlock.input_buffer, sampleBlock.output_buffer, String.Format("Collections are not equal for {0}", sampleBlock)); ;
            }

        }
    }
}
