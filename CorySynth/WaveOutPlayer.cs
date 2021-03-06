﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator
{
    public class WaveOutPlayer : IDisposable
    {
        private NAudio.Wave.WasapiOut waveOut;

        public event EventHandler<NAudio.Wave.StoppedEventArgs> PlaybackStopped;

        public int Latency { get; private set; }

        public WaveOutPlayer(int latency)
        {
            Latency = latency;
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
                waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, Latency);
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
                waveOut.PlaybackStopped -= waveOut_PlaybackStopped; ;
                waveOut.Dispose();
                waveOut = null;
            }
        }

        public void Dispose()
        {
            DisposeWaveOut();
        }
    }
}
