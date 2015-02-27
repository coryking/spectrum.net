﻿using CorySignalGenerator.Models;
using NAudio.CoreAudioApi;
using NAudio.Wave;
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
    public class WaveOutPlayer : PropertyChangeModel, IDisposable
    {
        private NAudio.Win8.Wave.WaveOutputs.WasapiOutRT waveOut;

        public event EventHandler<NAudio.Wave.StoppedEventArgs> PlaybackStopped;

        public int Latency { get; private set; }

        public WaveOutPlayer(int latency)
        {
            Latency = latency;
            watcher = new DeviceWatchWrapper(DeviceClass.AudioRender);
            watcher.DevicesChanged += watcher_DevicesChanged;
        }

        
        public bool IsActive
        {
            get
            {
                return (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing);
            }
        }

        public PlaybackState State
        {
            get {
                if (waveOut == null)
                    return PlaybackState.Stopped;
                return waveOut.PlaybackState;
            }

        }


        public void StartPlayback(ISampleProvider provider)
        {
            if (waveOut == null)
            {
                if (SelectedAudioDevice == null)
                    waveOut = new NAudio.Win8.Wave.WaveOutputs.WasapiOutRT(NAudio.CoreAudioApi.AudioClientShareMode.Shared, Latency);
                else
                    waveOut = new NAudio.Win8.Wave.WaveOutputs.WasapiOutRT(SelectedAudioDevice.Id, NAudio.CoreAudioApi.AudioClientShareMode.Shared, Latency);
                waveOut.PlaybackStopped += waveOut_PlaybackStopped;
                waveOut.Init(provider);
                waveOut.Play();
            }
        }
        public void EndPlayback()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }
        }

        void waveOut_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            DisposeWaveOut();
            if (PlaybackStopped != null)
            {
                PlaybackStopped(this, e);
            }
        }

        private void DisposeWaveOut()
        {
            if (waveOut != null)
            {
                waveOut.PlaybackStopped -= waveOut_PlaybackStopped;
                waveOut.Dispose();
                waveOut = null;
            }
        }

        #region Audio Watcher

        protected CoreDispatcher Dispatcher
        {
            get
            {
                return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            }
        }

        async void audio_watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (SelectedAudioDevice == null)
                {
                    SelectedAudioDevice = AudioDevices.FirstOrDefault();
                }
            });
        }

        void watcher_DevicesChanged(DeviceWatchWrapper sender)
        {
            if (SelectedAudioDevice == null)
            {
                SelectedAudioDevice = AudioDevices.FirstOrDefault();
            }
        }



        public ObservableCollection<DeviceInformation> AudioDevices
        {
            get
            {
                return watcher.Devices;
            }
        }


        #region Property SelectedAudioDevice
        private DeviceInformation _selectedAudioDevice;
        /// <summary>
        /// Sets and gets the SelectedAudioDevice property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DeviceInformation SelectedAudioDevice
        {
            get
            {
                return _selectedAudioDevice;
            }
            set
            {
                Set(ref _selectedAudioDevice, value);
            }
        }
        #endregion

        private DeviceWatchWrapper watcher = null;

        #endregion

        public void Dispose()
        {
            DisposeWaveOut();
        }
    }
}
