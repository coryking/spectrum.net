using CorySignalGenerator.Models;
using CorySignalGenerator.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace CorySynthUI.Models
{
    public class SettingsViewModel : PropertyChangeModel, CorySynthUI.Models.ISettingsViewModel
    {
        /// <summary>
        /// Selected Midi Device ID
        /// </summary>
        public Device MidiCaptureDevice
        {
            get
            {
                return Device.FromStorage(GetSetting<ApplicationDataCompositeValue>(default(ApplicationDataCompositeValue)));
            }
            set
            {
                SaveSetting(value.ToStorage());
            }
        }

        /// <summary>
        /// Selected Audio Output Device ID
        /// </summary>
        public Device AudioRenderDevice
        {
            get
            {
                return Device.FromStorage(GetSetting<ApplicationDataCompositeValue>(default(ApplicationDataCompositeValue)));
            }
            set
            {
                SaveSetting(value.ToStorage());
            }
        }



        #region miscellaneous

        /// <summary>
        /// Gets a value from the roaming settings.
        /// </summary>
        /// <typeparam name="T">The type of the value expected.</typeparam>
        /// <param name="defaultValue">The default value to return if the setting does not exist.</param>
        /// <param name="setting">The name of the setting to retrieve; the default is the name of the calling property.</param>
        /// <returns>The value.</returns>
        private T GetSetting<T>(T defaultValue, [CallerMemberName] String setting = null)
        {
            return (T)(ApplicationData.Current.RoamingSettings.Values[setting] ?? defaultValue);
        }

        /// <summary>
        /// Saves a value to the roaming settings.
        /// </summary>
        /// <typeparam name="T">The type of the value to save.</typeparam>
        /// <param name="value">The value to save.</param>
        /// <param name="setting">The name of the setting to save the value under; the default is the name of the calling property.</param>
        private void SaveSetting<T>(T value, [CallerMemberName] String setting = null)
        {
            ApplicationData.Current.RoamingSettings.Values[setting] = value;
            UpdateStatus();
        }

        /// <summary>
        /// Updates UI bound to the Text properties.
        /// </summary>
        private void UpdateStatus()
        {
            OnPropertyChanged("MidiCaptureDevice");
            OnPropertyChanged("AudioRenderDevice");
        }

        /// <summary>
        /// Updates UI bound to any of the properties.
        /// </summary>
        public void UpdateSettings()
        {
            OnPropertyChanged("MidiCaptureDevice");
            OnPropertyChanged("AudioRenderDevice");
        }

      
        #endregion miscellaneous
    }
}
