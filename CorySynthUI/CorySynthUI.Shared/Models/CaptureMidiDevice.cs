using CorySignalGenerator.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WindowsPreview.Devices.Midi;

namespace CorySynthUI.Models
{
    public class CaptureMidiDevice : IMidiInputDevice, IDisposable
    {
        MidiInPort midiIn;

        public void Dispose()
        {
            disposeMidiIn();
        }
        void MidiInput_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            OnMessageReceived(sender, args);
        }

        private void OnMessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
            {
                var msg = CorySignalGenerator.Sequencer.Midi.MidiMessageConverter.ToMidiMessage(args.Message);
                if (msg != null)
                    MessageReceived(this, new CorySignalGenerator.Sequencer.Midi.MidiInputMessageEventArgs(msg));
            }
        }

        public event Windows.Foundation.TypedEventHandler<IMidiInputDevice, CorySignalGenerator.Sequencer.Midi.MidiInputMessageEventArgs> MessageReceived;

        public string DeviceId
        {
            get;
            private set;
        }

        public void ChangeDevice(Device newDevice)
        {
            if (newDevice == null)
            {
                disposeMidiIn();
                return;
            }
            ChangeDevice(newDevice.Id);
        }

        public void ChangeDevice(Windows.Devices.Enumeration.DeviceInformation newDevice)
        {
            ChangeDevice(newDevice.Id);
        }

        public void ChangeDevice(string newDeviceId)
        {
            Task.Run(() => initMidiDeviceAsync(newDeviceId));
        }

        private async void initMidiDeviceAsync(string deviceId)
        {
            disposeMidiIn();
            
            midiIn = await MidiInPort.FromIdAsync(deviceId);
            midiIn.MessageReceived += MidiInput_MessageReceived;
        }

        private void disposeMidiIn()
        {
            if (midiIn != null)
            {
                midiIn.MessageReceived -= MidiInput_MessageReceived;
                midiIn.Dispose();
            }
        }

    }
}
