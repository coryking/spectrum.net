using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Oscillator
{
    public interface IBaseFunction
    {
        /// <summary>
        /// Process a value
        /// </summary>
        /// <param name="x">X Value</param>
        /// <param name="a">Parameter Value</param>
        /// <returns>The result</returns>
        float Process(float x, float a);

        /// <summary>
        /// Name of this base function
        /// </summary>
        string Name { get; }
    }
}
