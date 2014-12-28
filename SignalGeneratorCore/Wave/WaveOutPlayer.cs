using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGeneratorCore.Wave
{
    public class WaveOutPlayer :IDisposable
    {
        private NAudio.Win8.Wave.WaveOutputs.WasapiOutRT waveOut;
        public WaveOutPlayer()
        {

        }

        public bool IsActive
        {
            get
            {
                return (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing);
            }
        }


        public void StartPlayback(ISampleProvider provider)
        {
            if (waveOut == null)
            {
                waveOut = new NAudio.Win8.Wave.WaveOutputs.WasapiOutRT(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 10);
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
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
