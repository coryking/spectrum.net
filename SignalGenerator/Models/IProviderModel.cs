using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{
    public interface IProviderModel
    {
        ISampleProvider GetProvider(float frequency, int velocity, int sampleRate, int channels);
    }
}
