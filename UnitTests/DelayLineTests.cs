using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Dsp;

namespace UnitTests
{
    [TestClass]
    public class DelayLineTests
    {

        [TestMethod]
        public void TestDelayLine_to_same_channel()
        {
            var decayFactor = 0.5f;
            var delayLine = new DelayLine(44100) { Decay = decayFactor, SampleDelay = 1, Channels=2 };

            var buffers = new float[][]{
                new float[]{1,0, 0,0},
                new float[]{2,0, 0,0},
                new float[]{0,0, 0,0},
                new float[]{0,0, 0,0},
            };

            var expectedResults = new float[][]{
                 new float[]{ 0,0, 1 * decayFactor, 0,  },
                new float[]{ 0,0, 2 * decayFactor, 0, },
                new float[]{ 0, 0, 0, 0 },
                new float[]{ 0f, 0, 0, 0 },
            };
            TestDelayLineStage(buffers, expectedResults, (i, lastItem, input, output) =>
            {
                var amountRead = delayLine.Write(input, 0, 4, lastItem);
                Assert.AreEqual(4, amountRead);

                var amountWritten = 0;
                if (lastItem)
                    amountWritten = delayLine.Read(output, 0, 4);
                else
                    amountWritten = delayLine.Read(output, 0, 4);

                Assert.AreEqual(4, amountWritten, String.Format("Item: {0}", i));
            });

        }

        [TestMethod]
        public void TestDelayLine_to_same_channel_diff_order()
        {
            // same as similar test but we change the order or read/write.
            var decayFactor = 0.5f;
            var delayLine = new DelayLine(44100) { Decay = decayFactor, SampleDelay = 1, Channels = 2 };

            var buffers = new float[][]{
                new float[]{1,0, 0,0},
                new float[]{2,0, 0,0},
                new float[]{0,0, 0,0},
                new float[]{0,0, 0,0},
            };

            var expectedResults = new float[][]{
                new float[]{ 0, 0, 0, 0 }, // since we read from the delay line before writing to it, the first block will be empty...
                new float[]{ 0,0, 1 * decayFactor, 0,  }, // now everything should be staggered...
                new float[]{ 0,0, 2 * decayFactor, 0, },
                new float[]{ 0f, 0, 0, 0 },

            };
            TestDelayLineStage(buffers, expectedResults, (i, lastItem, input, output) =>
            {
                var amountWritten = 0;
                if (lastItem)
                    amountWritten = delayLine.Read(output, 0, 4);
                else
                    amountWritten = delayLine.Read(output, 0, 4);

                Assert.AreEqual(4, amountWritten, String.Format("Item: {0}", i));

                var amountRead = delayLine.Write(input, 0, 4, lastItem);
                Assert.AreEqual(4, amountRead);
            });

        }

        [TestMethod]
        public void TestDelayLine_to_diff_channel()
        {
            var decayFactor = 0.5f;
            var delayLine = new DelayLine(44100) { Decay = decayFactor, SampleDelay = 1, Channels = 2, FromChannel=0, ToChannel=1 };

            var buffers = new float[][] {
                new float[]{ 1, 0, 0, 0 },
                new float[]{ 2, 0, 0, 0 },
                new float[]{ 0, 0, 0, 0 },
                new float[]{ 0, 0, 0, 0 }
            };

            var expectedResults = new float[][]{
                new float[]{0,0, 0, 1 * decayFactor, },
                new float[]{0,0, 0, 2 * decayFactor, },
                new float[]{0, 0, 0, 0},
                new float[]{0, 0, 0, 0},
            };

            TestDelayLineStage(buffers, expectedResults, (i, lastItem, input, output) =>
            {
                var amountRead = delayLine.Write(input, 0, 4);
                Assert.AreEqual(4, amountRead);

                var amountWritten = delayLine.Read(output, 0, 4);
                Assert.AreEqual(4, amountWritten, String.Format("Item: {0}", i));
            });

        }

        public static void TestDelayLineStage(float[][] input, float[][] expectedResults, Action<int, bool,float[], float[]> function)
        {
            var output = new float[input.Length][];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = new float[input[i].Length];
                bool lastItem = (i == input.Length - 1);
                function.Invoke(i, lastItem, input[i], output[i]);
            }
            for (int i = 0; i < expectedResults.Length; i++)
            {
                CollectionAssert.AreEqual(expectedResults[i], output[i], String.Format("Line {0}", i));
            }
        }
    }
}
