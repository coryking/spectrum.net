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
    public class Device
    {
        public Device(string name, string id, bool isenabled)
        {
            Name = name;
            Id = id;
            IsEnabled = isenabled; ;
        }

        public static Device FromDeviceInformation(DeviceInformation information)
        {
            return new Device(information.Name, information.Id, information.IsEnabled);
        }

        public Windows.Storage.ApplicationDataCompositeValue ToStorage()
        {
            var composite = new Windows.Storage.ApplicationDataCompositeValue();
            composite["id"] = Id;
            composite["name"] = Name;
            return composite;
        }

        public static Device FromStorage(Windows.Storage.ApplicationDataCompositeValue composite)
        {
            if (composite == null)
                return null;
            var id = composite["id"] as string;
            var name = composite["name"] as string;
            return new Device(name, id, true);
        }



        public Boolean IsEnabled { get; set; }
        public String Name { get; private set; }
        public String Id { get; private set; }
    }

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
            Devices = new ObservableCollection<Device>();

            _watcher = watcher;
            _watcher.Added += audio_watcher_Added;
            _watcher.Removed += audio_watcher_Removed;
            
            // Nothing we do with updated... maybe sometime in the future
            //_watcher.Updated += audio_watcher_Updated;
            _watcher.EnumerationCompleted += audio_watcher_EnumerationCompleted;
            _watcher.Start();
        }

        public DeviceWatchWrapper(string deviceSelector)
            : this(DeviceInformation.CreateWatcher(deviceSelector))
        {
            //midiSelector = WindowsPreview.Devices.Midi.MidiInPort.GetDeviceSelector();
        }

        public DeviceWatchWrapper(DeviceClass deviceClass)
            : this(DeviceInformation.CreateWatcher(deviceClass))
        {
        }

        protected CoreDispatcher Dispatcher
        {
            get
            {
                return Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher;
            }
        }


        public event EventHandler<DeviceChangedEventArgs<Device>> SelectedDeviceChanged;

        #region Properties
        public ObservableCollection<Device> Devices { get; private set; }


        #region Property SelectedDevice
        private Device _selectedDevice = null;

        /// <summary>
        /// Sets and gets the SelectedDevice property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Device SelectedDevice
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
            SelectedDevice = Device.FromDeviceInformation(device);
            return device;
        }

        #endregion

        #endregion


        #region _watch events

        void audio_watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {

            if (SelectedDevice == null || !Devices.Contains(SelectedDevice))
            {
                SelectedDevice = Devices.FirstOrDefault();
            }
        }

        void audio_watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {

            var device = Devices.Where(x => x.Id == args.Id).FirstOrDefault();

            if (device != null)
                Caliburn.Micro.Execute.OnUIThread(() => Devices.Remove(device));
        }

        void audio_watcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if (args.IsEnabled)
                Caliburn.Micro.Execute.OnUIThread(() => Devices.Add(Device.FromDeviceInformation(args)));
        }

        #endregion


        protected void OnSelectedDeviceChanged(Device oldValue, Device newValue)
        {
            if (SelectedDeviceChanged != null)
                SelectedDeviceChanged(this, new DeviceChangedEventArgs<Device>(oldValue, newValue));
        }
    }
}
