using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorySignalGenerator.Utils;

namespace UnitTests
{
    [TestClass]
    public class FrequencyUtilsTests
    {
        [TestMethod]
        public void TestScalePositive()
        {
            var expectedFrequency = 27.5 * 2;

            var actualFrequency = FrequencyUtils.ScaleFrequency(27.5f, 12f, 12f);

            Assert.AreEqual(expectedFrequency, actualFrequency);

        }

        [TestMethod]
        public void TestScaleNegative()
        {
            var expectedFrequency = 27.5 / 2;
            var actualFrequency = FrequencyUtils.ScaleFrequency(27.5f, -12f, 12f);

            Assert.AreEqual(expectedFrequency, actualFrequency);
            
        }

        [TestMethod]
        public void TestScaleZero()
        {
            var expectedFrequency = 27.5;
            var actualFrequency = FrequencyUtils.ScaleFrequency(27.5f, 0, 12f);

            Assert.AreEqual(expectedFrequency, actualFrequency);

        }
    }
}
