using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer.Interfaces
{
    /// <summary>
    /// A sample provider that has a source
    /// </summary>
    public interface ISourcedSampleProvider : ISampleProvider
    {

        /// <summary>
        /// Gets or sets the source of this sample provider
        /// </summary>
        ISampleProvider Source { get; set; }
    }
}
