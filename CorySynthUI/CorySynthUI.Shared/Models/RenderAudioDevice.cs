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


        public void ChangeAudioDevice(Windows.Devices.Enumeration.DeviceInformation newDevice)
        {
            if (newDevice == null)
            {
                DeviceId = string.Empty;
                destroyOldWaveOut();
                return;
            }
            ChangeAudioDevice(newDevice.Id);
        }
        public void ChangeAudioDevice(Device newDevice)
        {
            if (newDevice == null)
            {
                destroyOldWaveOut();
                DeviceId = string.Empty;
                return;
            }


            ChangeAudioDevice(newDevice.Id);
        }

        public void ChangeAudioDevice(string newDeviceId)
        {
            DeviceId = newDeviceId;
            reInitWaveOut();

        }

        private void reInitWaveOut()
        {
            destroyOldWaveOut();
            initWaveOut();
        }

        private void destroyOldWaveOut()
        {
            lock (_lock)
            {
                if (waveOut != null)
                {
                    waveOut.Dispose();
                    waveOut = null;
                    Debug.WriteLine("Done  knocking out waveout");
                }
            }
        }

        private void initWaveOut()
        {
            Debug.WriteLine("Init wave out");
            // bail if there is no provider or deviceid
            if (SampleProvider == null || String.IsNullOrEmpty(DeviceId))
                return;
            Debug.WriteLine("Going to wait for lock");
            lock (_lock)
            {
                Debug.WriteLine("Inside lock for initwaveout");
                Debug.Assert(waveOut == null);

                if (waveOut != null)
                    return;
                waveOut = new NAudio.Win8.Wave.WaveOutputs.WasapiOutRT(DeviceId, NAudio.CoreAudioApi.AudioClientShareMode.Shared, Latency);
                waveOut.Init(SampleProvider);
                waveOut.Play();
            }
        }


        public void ChangeSampleProvider(ISampleProvider newProvider)
        {
            Debug.WriteLine("Changing Sample Provider");
            SampleProvider = newProvider;
            reInitWaveOut();

        }



        public void Dispose()
        {
            var wave = waveOut as IDisposable;
            if (wave != null)
                wave.Dispose();

        }



    }
}
