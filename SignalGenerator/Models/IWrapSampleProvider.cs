using System;
namespace CorySignalGenerator.Models
{
    public interface IWrapSampleProvider
    {
        NAudio.Wave.ISampleProvider WrapProvider(NAudio.Wave.ISampleProvider source);
    }
}
