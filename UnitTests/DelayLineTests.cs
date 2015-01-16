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
                new float[]{ 1, 0, 0, 0 },
                new float[]{ 2.5f, 0, 0, 0 },
                new float[]{ 1.25f, 0, 0, 0 },
                new float[]{ 0.625f, 0, 0, 0 },
            };
            TestDelayLineStage(buffers, expectedResults, (lastItem, buffer) =>
            {
                if(lastItem)
                    delayLine.ConvolveDelayLine(buffer, 0, buffer,0, 4, 0, decayFactor, 2, 0, 0, 2);
                else
                    delayLine.ConvolveDelayLine(buffer, 0, buffer,0, 4, 4, decayFactor, 2, 0, 0, 2);
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

            TestDelayLineStage(buffers, expectedResults, (lastItem, floats) =>
            {
                delayLine.ConvolveDelayLine(floats, 0, floats,0, 4, 4, decayFactor, 2, 0, 1, 2);
            });

        }

        public static void TestDelayLineStage(float[][] buffers, float[][] expectedResults, Action<bool,float[]> function)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                bool lastItem = (i == buffers.Length - 1);
                function.Invoke(lastItem, buffers[i]);
            }
            for (int i = 0; i < expectedResults.Length; i++)
            {
                CollectionAssert.AreEqual(expectedResults[i], buffers[i], String.Format("Line {0}", i));
            }
        }
    }
}
