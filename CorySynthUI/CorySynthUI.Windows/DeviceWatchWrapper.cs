using CorySignalGenerator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace CorySynthUI
{
    public delegate void DevicesChangedEventArgs(DeviceWatchWrapper sender);

    public class DeviceWatchWrapper : PropertyChangeModel
    {
        private DeviceWatcher _watcher;
        public DeviceWatchWrapper(string deviceSelector)
        {
            //midiSelector = WindowsPreview.Devices.Midi.MidiInPort.GetDeviceSelector();
            Devices = new ObservableCollection<DeviceInformation>();
            _watcher = DeviceInformation.CreateWatcher(deviceSelector);
            _watcher.Added += audio_watcher_Added;
            _watcher.Removed += audio_watcher_Removed;
            _watcher.Updated += audio_watcher_Updated;
            _watcher.EnumerationCompleted += audio_watcher_EnumerationCompleted;
            _watcher.Start();
        }

        public event DevicesChangedEventArgs DevicesChanged;

        #region Properties
        public ObservableCollection<DeviceInformation> Devices { get; private set; }

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
    }
}
