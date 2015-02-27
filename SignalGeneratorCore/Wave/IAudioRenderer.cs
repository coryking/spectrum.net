using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace CorySignalGenerator.Wave
{
    public interface IAudioRenderer
    {
        ISampleProvider SampleProvider { get; }
        
        int Latency { get; }

        string DeviceId { get; }

        void ChangeAudioDevice(DeviceInformation newDevice);
        void ChangeAudioDevice(string newDeviceId);
        void ChangeSampleProvider(ISampleProvider newProvider);

    }
}
