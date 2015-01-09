using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

namespace UnitTests
{
    [TestClass]
    public abstract class BaseShimTests
    {
        IDisposable shimContext;

        public float[] GetData(int size, Func<int,float> action)
        {
            var data = new float[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = action.Invoke(i);
            }
            return data;
        }


        [TestInitialize]
        public void Startup()
        {
            shimContext = ShimsContext.Create();
        }

        [TestCleanup]
        public void Teardown()
        {
            shimContext.Dispose();
        }
    }
}
