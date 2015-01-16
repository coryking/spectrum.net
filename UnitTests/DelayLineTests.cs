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
            var delayLine = new DelayLine(44100);
            var decayFactor = 0.5f;
            var buffers = new float[][]{
                new float[]{1,0, 0,0},
                new float[]{2,0, 0,0},
                new float[]{0,0, 0,0},
                new float[]{0,0, 0,0},
            };

            var expectedResults = new float[][]{
                new float[]{ 0, 0, 0, 0 },
                new float[]{ 1 * decayFactor, 0, 0, 0 },
                new float[]{ 2 * decayFactor, 0, 0, 0 },
                new float[]{ 0f, 0, 0, 0 },
            };
            TestDelayLineStage(buffers, expectedResults, (i, lastItem, input, output) =>
            {
                var amountRead = 0;
                if (lastItem)
                    amountRead = delayLine.ConvolveDelayLine(input, 0, output, 0, 4, 0, decayFactor, 2, 0, 0, 2);
                else
                    amountRead = delayLine.ConvolveDelayLine(input, 0, output, 0, 4, 4, decayFactor, 2, 0, 0, 2);

                Assert.AreEqual(2, amountRead, String.Format("Item: {0}", i));
            });

        }

        [TestMethod]
        public void TestDelayLine_to_diff_channel()
        {
            var delayLine = new DelayLine(44100);
            var decayFactor = 0.5f;

            var buffers = new float[][] {
                new float[]{ 1, 0, 0, 0 },
                new float[]{ 2, 0, 0, 0 },
                new float[]{ 0, 0, 0, 0 },
                new float[]{ 0, 0, 0, 0 }
            };

            var expectedResults = new float[][]{
                new float[]{0, 0, 0, 0},
                new float[]{0, 1 * decayFactor, 0, 0},
                new float[]{0, 2 * decayFactor, 0, 0},
                new float[]{0, 0, 0, 0},
            };

            TestDelayLineStage(buffers, expectedResults, (i, lastItem, input, output) =>
            {
                var amountRead = delayLine.ConvolveDelayLine(input, 0, output,0, 4, 4, decayFactor, 2, 0, 1, 2);
                Assert.AreEqual(2, amountRead, String.Format("Item: {0}", i));
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
