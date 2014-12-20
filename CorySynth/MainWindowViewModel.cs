using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
namespace CorySynth
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Channels = AudioChannels.MONO;
            Generator = new SignalGenerator(SampleRate, (int)Channels);
            Adsr = new Models.Adsr();
        }

        public Models.Adsr Adsr { get; set; }

        public SignalGenerator Generator { get; set; }

        
        public int SampleRate { get { return 44100; } }

        public AudioChannels Channels { get; set; }

        public ISampleProvider GetAudioChain()
        {
            var adsrProvider = new AdsrSampleProvider(Generator)
            {
                AttackSeconds=Adsr.AttackSeconds,
                ReleaseSeconds = Adsr.ReleaseSeconds
            };
            return adsrProvider;
        }

    }

    public enum AudioChannels
    {
        [Description("Mono")]
        MONO=1,
        [Description("Stereo")]
        STEREO=2,
    }
}
