using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace CorySynthWinStore
{
    public delegate void MidiDevicesChangedEventArgs(MidiDeviceWatcher sender);

    public class MidiDeviceWatcher
    {
        private DeviceWatcher _watcher;
        private DeviceInformationCollection _deviceInfoCollection;
        private String midiSelector;
        private bool _enumCompleted;
        public MidiDeviceWatcher()
        {
            midiSelector = WindowsPreview.Devices.Midi.MidiInPort.GetDeviceSelector();
            _watcher = DeviceInformation.CreateWatcher(midiSelector);
            _watcher.Added += _watcher_Added;
            _watcher.Removed += _watcher_Removed;
            _watcher.Updated += _watcher_Updated;
            _watcher.EnumerationCompleted += _watcher_EnumerationCompleted;

        }

        public event MidiDevicesChangedEventArgs MidiDevicesChanged;

        #region _watch events

        void _watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (_enumCompleted)
                UpdatePorts();
        }

        void _watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            _enumCompleted = true;
            UpdatePorts();
        }

        void _watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (_enumCompleted)
                UpdatePorts();
        }

        void _watcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if (_enumCompleted)
                UpdatePorts();
        }
        #endregion

        public void Start()
        {
            _watcher.Start();

        }
        public void Stop()
        {
            _watcher.Stop();
        }

        public void UpdatePorts()
        {
            Task.Run(async () =>
            {
                _deviceInfoCollection = await DeviceInformation.FindAllAsync(WindowsPreview.Devices.Midi.MidiInPort.GetDeviceSelector());
            }).Wait();
            OnMidiDevicesChanged();
        }

        public DeviceInformationCollection GetDeviceInformationCollection()
        {
            return _deviceInfoCollection;
        }

        protected void OnMidiDevicesChanged()
        {
            if (MidiDevicesChanged != null)
                MidiDevicesChanged(this);
        }
    }
}
