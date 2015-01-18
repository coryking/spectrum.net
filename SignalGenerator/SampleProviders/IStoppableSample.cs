using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    public interface IStoppableSample
    {
        /// <summary>
        /// Stop Playing the sound (or begin stopping)
        /// </summary>
        void Stop();

        event EventHandler SampleHasStopped; 
    }
}
