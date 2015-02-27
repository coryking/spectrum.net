using CorySignalGenerator.Wave;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CorySynthUI.Models
{
    /// <summary>
    /// The audio device we play to
    /// </summary>
    public class RenderAudioDevice :IAudioRenderer, IDisposable
    {
        private NAudio.Win8.Wave.WaveOutputs.WasapiOutRT waveOut;
        private object _lock = new object();
        public RenderAudioDevice(int latency)
        {
            Latency = latency;
        }

        public ISampleProvider SampleProvider
        {
            get;
            private set;
        }

        public int Latency
        {
            get;
            private set;
        }

        public String DeviceId
        {
            get;
            private set;
        }
        private CoreDispatcher Dispatcher
        {
            get
            {
                return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            }
        }


        public void ChangeAudioDevice(Windows.Devices.Enumeration.DeviceInformation newDevice)
        {
            ChangeAudioDevice(newDevice.Id);
        }

        public void ChangeAudioDevice(string newDeviceId)
        {
            reInitWaveOut(newDeviceId, SampleProvider);

        }

        private void reInitWaveOut(string deviceId, ISampleProvider provider)
        {
            if (waveOut != null)
            {
                Monitor.Enter(_lock);
                waveOut.Stop();
                waveOut.PlaybackStopped += async (sender, e) =>
                {
                    waveOut.Dispose();
                    waveOut = null;
                    Monitor.Exit(_lock); // there is probaly some chance the callback will never happen and this lock will never be freed
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        initWaveOut(deviceId, provider);
                    });
                };
            }
            else
            {
                initWaveOut(deviceId,provider);
            }

        }

        private void initWaveOut(string deviceId, ISampleProvider provider)
        {
            // bail if there is no provider or deviceid
            if (provider == null || String.IsNullOrEmpty(deviceId))
                return;
            lock (_lock)
            {
                Debug.Assert(waveOut == null);

                if (waveOut != null)
                    return;
                waveOut = new NAudio.Win8.Wave.WaveOutputs.WasapiOutRT(deviceId, NAudio.CoreAudioApi.AudioClientShareMode.Shared, Latency);
                waveOut.Init(provider);
                waveOut.Play();
                SampleProvider = provider;
                DeviceId = deviceId;
            }
        }


        public void ChangeSampleProvider(ISampleProvider newProvider)
        {
            SampleProvider = newProvider;
            reInitWaveOut(DeviceId, newProvider);

        }



        public void Dispose()
        {
            var wave = waveOut as IDisposable;
            if (wave != null)
                wave.Dispose();

        }
    }
}
