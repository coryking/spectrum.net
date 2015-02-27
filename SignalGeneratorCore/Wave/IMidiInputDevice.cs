using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace CorySignalGenerator.Wave
{
    public interface IMidiInputDevice
    {
        event TypedEventHandler<IMidiInputDevice, Sequencer.Midi.MidiInputMessageEventArgs> MessageReceived;

        string DeviceId { get; }

        void ChangeDevice(DeviceInformation newDevice);
        void ChangeDevice(Device newDevice);
        void ChangeDevice(string newDeviceId);
    }
}
