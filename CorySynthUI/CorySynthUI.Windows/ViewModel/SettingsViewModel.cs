using CorySignalGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorySynthUI.ViewModel
{
    public class SettingsViewModel : PropertyChangeModel
    {
        /// <summary>
        /// Selected Midi Device ID
        /// </summary>
        public String MidiDeviceId
        {
            get
            {
                return GetSetting<string>(String.Empty);
            }
            set
            {
                SaveSetting<string>(value);
            }
        }

        /// <summary>
        /// Selected Audio Output Device ID
        /// </summary>
        public String AudioOutputDeviceId
        {
            get
            {
                return GetSetting<string>(String.Empty);
            }
            set
            {
                SaveSetting<string>(value);
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
            OnPropertyChanged("NewGameOpponentsText");
            OnPropertyChanged("NewGameBoardSizeText");
        }

        /// <summary>
        /// Updates UI bound to any of the properties.
        /// </summary>
        public void UpdateSettings()
        {
            OnPropertyChanged("IsLastMoveIndicatorShowing");
            OnPropertyChanged("IsShowingValidMoves");
            OnPropertyChanged("IsClockShowing");
            OnPropertyChanged("PlayerOneIsAiSetting");
            OnPropertyChanged("PlayerTwoIsAiSetting");
            OnPropertyChanged("PlayerOneAiSearchDepthSetting");
            OnPropertyChanged("PlayerTwoAiSearchDepthSetting");
            OnPropertyChanged("BoardSizeIndex");
            OnPropertyChanged("NewGameOpponentsText");
            OnPropertyChanged("NewGameBoardSizeText");
        }

      
        #endregion miscellaneous
    }
}
