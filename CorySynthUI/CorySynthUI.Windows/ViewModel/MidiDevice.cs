using CorySignalGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using WindowsPreview.Devices.Midi;

namespace CorySynthUI.ViewModel
{
    public class DeviceChangedEventArgs<T>
    {
        public DeviceChangedEventArgs(T oldDevice, T newDevice)
        {
            OldDevice = oldDevice;
            NewDevice = newDevice;
        }

        public T OldDevice { get; private set; }
        public T NewDevice { get; private set; }
    }

    public sealed class MidiDevice : PropertyChangeModel, IDisposable
    {
        public MidiDevice()
        {
            MidiDevices = new ObservableCollection<DeviceInformation>();
            MidiWatcher = new MidiDeviceWatcher();
            MidiWatcher.MidiDevicesChanged += MidiWatcher_MidiDevicesChanged;
            MidiWatcher.Start();
        }

        #region Event Handlers

        async void MidiWatcher_MidiDevicesChanged(MidiDeviceWatcher sender)
        {
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            var items = sender.GetDeviceInformationCollection().Where(x => x.IsEnabled).OrderBy(x => x.Name);
            await dispatcher.RunIdleAsync((e) =>
            {
                MidiDevices.Clear();
                foreach (var item in items)
                {
                    MidiDevices.Add(item);

                }
            });
            
        }
        void MidiInput_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            OnMessageReceived(sender, args);
        }



        #endregion

        #region Events

        public event TypedEventHandler<MidiInPort, MidiMessageReceivedEventArgs> MessageReceived;
        private void OnMessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
                MessageReceived(sender, args);
        }

        public event EventHandler<DeviceChangedEventArgs<DeviceInformation>> MidiDeviceChanged;
        private async void OnMidiDeviceChangedAsync(DeviceInformation oldDevice, DeviceInformation newDevice)
        {
            if(MidiDeviceChanged != null)
            {
                MidiDeviceChanged(this, new DeviceChangedEventArgs<DeviceInformation>(oldDevice, newDevice));
            }
            if (newDevice != null)
            {
                MidiInput = await MidiInPort.FromIdAsync(newDevice.Id);
            }
            else
            {
                MidiInput = null;
            }
            
        }

        public event EventHandler<DeviceChangedEventArgs<MidiInPort>> MidiInPortChanged;

        private void OnMidiInPortChanged(MidiInPort oldInput, MidiInPort newInput)
        {
            if (oldInput == newInput)
                return;
            if(oldInput != null)
            {
                oldInput.MessageReceived -= MidiInput_MessageReceived;
                oldInput.Dispose();
            }
            if (newInput != null)
            {
                newInput.MessageReceived += MidiInput_MessageReceived;
            }
            if(MidiInPortChanged != null)
            {
                MidiInPortChanged(this, new DeviceChangedEventArgs<MidiInPort>(oldInput, newInput));
            }
        }

        

        #endregion

        #region Properties
        #region Property SelectedMidiDevice
        private DeviceInformation _selectedMidiDevice = null;

        /// <summary>
        /// Sets and gets the SelectedMidiDevice property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DeviceInformation SelectedMidiDevice
        {
            get
            {
                return _selectedMidiDevice;
            }
            set
            {
                var oldValue = _selectedMidiDevice;
                Set(ref _selectedMidiDevice, value);
                OnMidiDeviceChangedAsync(oldValue, value);
            }
        }
        #endregion

        public ObservableCollection<DeviceInformation> MidiDevices { get; private set; }


        protected MidiDeviceWatcher MidiWatcher { get; private set; }

        
        #region Property MidiInput
        private MidiInPort _midiInput = null;

        /// <summary>
        /// Sets and gets the MidiInput property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MidiInPort MidiInput
        {
            get
            {
                return _midiInput;
            }
            set
            {
                var oldValue = _midiInput;
                Set(ref _midiInput, value);
                OnMidiInPortChanged(oldValue, value);
            }
        }
        #endregion
		


        #endregion


        public void Dispose()
        {
            if (MidiInput != null)
            {
                MidiInput.MessageReceived -= MidiInput_MessageReceived;
                MidiInput.Dispose();
            }
        }
    }
}
