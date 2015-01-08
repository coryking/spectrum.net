using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.SampleProviders
{
    public delegate int SampleProviderRead(float[] buffer, int offset, int count);

    /// <summary>
    /// A sample provider that calls <see cref="Function"/>
    /// </summary>
    public class FuncSampleProvider :ISampleProvider
    {
        public FuncSampleProvider(WaveFormat format,SampleProviderRead readFunction)
        {
            WaveFormat = format;
            Function = readFunction;

        }

        public SampleProviderRead Function { get; set; }


        public int Read(float[] buffer, int offset, int count)
        {
            Debug.Assert(Function != null);
            if (Function == null)
                return 0;

            return Function.Invoke(buffer, offset, count);
        }

        public WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
