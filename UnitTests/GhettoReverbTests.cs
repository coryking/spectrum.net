using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CorySignalGenerator.Filters;
using CorySignalGenerator.SampleProviders;
namespace UnitTests
{
    [TestClass]
    public class GhettoReverbTests
    {
        [Ignore] // ignore for now... this gets to crazy to test
        [TestMethod]
        public void TestGhettoReverb()
        {
            var decayFactor = 0.5f;
            var secondDecayFactor = 0.5f * decayFactor;
            var buffers = new float[][]{
                new float[]{1,0, 0,0},
                new float[]{2,0, 0,0},
                new float[]{0,0, 0,0},
                new float[]{0,0, 0,0},
                new float[]{0,1,0,0},
                new float[]{0,2,0,0},
                new float[]{0,0, 0,0},
                new float[]{0,0, 0,0},

            };

            var actualResults = new float[][]{
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},
                new float[]{0,0,0,0},

            };

            var expectedResults = new float[][]{
                new float[]{ 1, 0, 0, 0 },
                new float[]{ 2.5f, 0.25f, 0, 0 },
                new float[]{ 1f, 0.5f, 0, 0 },
                new float[]{ 0, 0, 0, 0 },
                new float[]{ 0, 1, 0, 0 },
                new float[]{ 0.25f, 2.5f, 0, 0 },
                new float[]{ 0.5f, 1f, 0, 0 },
                new float[]{ 0, 0, 0, 0 },
            };
            var n = 0;

            var funcProvider = new FuncSampleProvider(NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100,2), (buffer,offset,count)=>
            {
                Array.Copy(buffers[n], buffer, count);
                n++;
                return 4;
            });

            var reverbFilter = new GhettoReverb(funcProvider);
            reverbFilter.Decay = decayFactor;
            reverbFilter.SecondaryDecay = secondDecayFactor;
            reverbFilter.SampleSecondaryDelayLeft = 0;
            reverbFilter.SampleSecondaryDelayRight = 0;
            reverbFilter.SampleDelay = 2;
            

            for (int i = 0; i < buffers.Length; i++)
			{
                var amountRead = reverbFilter.Read(actualResults[i], 0, 4);
                Assert.AreEqual(4, amountRead);
			}

            for (int i = 0; i < expectedResults.Length; i++)
            {
                CollectionAssert.AreEqual(expectedResults[i], actualResults[i], String.Format("Item {0}", i));
            }

        }
    }
}
