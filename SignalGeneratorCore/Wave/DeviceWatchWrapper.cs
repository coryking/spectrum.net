using CorySignalGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace CorySignalGenerator.Wave
{
    public delegate void DevicesChangedEventArgs(DeviceWatchWrapper sender);

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

    public class DeviceWatchWrapper : PropertyChangeModel
    {
        private DeviceWatcher _watcher;
        private DeviceWatchWrapper(DeviceWatcher watcher)
        {
            Devices = new ObservableCollection<DeviceInformation>();

            _watcher = watcher;
            _watcher.Added += audio_watcher_Added;
            _watcher.Removed += audio_watcher_Removed;
            _watcher.Updated += audio_watcher_Updated;
            _watcher.EnumerationCompleted += audio_watcher_EnumerationCompleted;
            _watcher.Start();
        }

        public DeviceWatchWrapper(string deviceSelector) : this(DeviceInformation.CreateWatcher(deviceSelector))
        {
            //midiSelector = WindowsPreview.Devices.Midi.MidiInPort.GetDeviceSelector();
        }

        public DeviceWatchWrapper(DeviceClass deviceClass) : this(DeviceInformation.CreateWatcher(deviceClass))
        {
        }

        public event DevicesChangedEventArgs DevicesChanged;

        public event EventHandler<DeviceChangedEventArgs<DeviceInformation>> SelectedDeviceChanged;

        #region Properties
        public ObservableCollection<DeviceInformation> Devices { get; private set; }


        #region Property SelectedDevice
        private DeviceInformation _selectedDevice = null;

        /// <summary>
        /// Sets and gets the SelectedDevice property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DeviceInformation SelectedDevice
        {
            get
            {
                return _selectedDevice;
            }
            set
            {
                Set(ref _selectedDevice, value, changeCallback: OnSelectedDeviceChanged);
            }
        }

        public async Task<DeviceInformation> SetSelectedDeviceFromIdAsync(string deviceId)
        {
            var device = await DeviceInformation.CreateFromIdAsync(deviceId);
            SelectedDevice = device;
            return device;
        }

        #endregion
		

        protected CoreDispatcher Dispatcher
        {
            get
            {
                return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            }
        }

        #endregion


        #region _watch events

        async void audio_watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnDevicesChanged();
                if (SelectedDevice == null || !Devices.Contains(SelectedDevice))
                {
                    SelectedDevice = Devices.FirstOrDefault();
                }
            });
        }

        async void audio_watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var device = Devices.Where(x => x.Id == args.Id).FirstOrDefault();
                if (device != null)
                    device.Update(args);
            });
        }

        async void audio_watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var device = Devices.Where(x => x.Id == args.Id).FirstOrDefault();

                if (device != null)
                    Devices.Remove(device);
            });
        }

        async void audio_watcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if (args.IsEnabled)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Devices.Add(args);
                });
        }

        #endregion

        protected void OnDevicesChanged()
        {
            if (DevicesChanged != null)
                DevicesChanged(this);
        }

        protected void OnSelectedDeviceChanged(DeviceInformation oldValue, DeviceInformation newValue)
        {
            if (SelectedDeviceChanged != null)
                SelectedDeviceChanged(this, new DeviceChangedEventArgs<DeviceInformation>(oldValue, newValue));
        }
    }
}
