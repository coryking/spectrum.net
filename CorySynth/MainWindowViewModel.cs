using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using NAudio.Midi;
using System.Windows.Threading;
using System.Timers;
using CorySignalGenerator.Models;
using CorySignalGenerator.Sounds;
using CorySignalGenerator.Sequencer;
using CorySignalGenerator.Filters;
using System.Diagnostics;
using CorySignalGenerator.SampleProviders;
namespace CorySignalGenerator
{
    public class MainViewModel : PropertyChangeModel
    {
        // Note about empirical tuning:
        // The maximum FFT size affects reverb performance and accuracy.
        // If the reverb is single-threaded and processes entirely in the real-time audio thread,
        // it's important not to make this too high.  In this case 8192 is a good value.
        // But, the Reverb object is multi-threaded, so we want this as high as possible without losing too much accuracy.
        // Very large FFTs will have worse phase errors. Given these constraints 32768 is a good compromise.
        static readonly int MaxFFTSize = 32768;



        private readonly int m_latency = 100; // Convert.ToInt32(Math.Pow(2, 13) / (44100 / 1000)) - 1;

        public const int TicksPerBeat = 24;
        public const double BeatsPerMinute = 60;


        private WaveOutPlayer _player;
        private Dispatcher _dispatcher;
        private NAudio.Midi.MidiIn _midiIn;
        
        #region Properties

       


        private bool _canPlay;
        public bool CanPlay
        {
            get { return _canPlay; }
            set { Set(ref _canPlay, value); }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { Set(ref _isPlaying, value); }
        }

        public MixingSampleProvider HeadSampleProvider { get; set; }

        public List<MidiInCapabilities> MidiDevices { get; private set; }

        public PadSound PadSound { get; set; }

        #endregion

        public MainViewModel(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            this.MidiDevices = new List<MidiInCapabilities>();
            for (var i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                MidiDevices.Add(MidiIn.DeviceInfo(i));
            }
        }

        void _player_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            CanPlay = true;
            IsPlaying = false;
        }

        private void BuildSignalChain()
        {

            HeadSampleProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            HeadSampleProvider.ReadFully = true;
            PadSound = new PadSound(HeadSampleProvider.WaveFormat);
            PadSound.Envelope = new AdsrEnvelopeEffectFactory();
            PadSound.InitSamples();
        }



        public void StartPlaying()
        {
            if (HeadSampleProvider != null)
            {
                _player.StartPlayback(HeadSampleProvider);
                IsPlaying = true;
            }
        }
        public void StopPlaying()
        {
            CanPlay = false;
            _player.EndPlayback();
            if (_midiIn != null)
            {
                _midiIn.Stop();
                _midiIn.MessageReceived -= _midiIn_MessageReceived;
                _midiIn.Dispose();
                _midiIn = null;
            }
        }

        public void StartListening(int deviceNo)
        {
            if (_midiIn == null)
            {
                _midiIn = new MidiIn(deviceNo);
                _midiIn.MessageReceived += _midiIn_MessageReceived;
                _midiIn.Start();
            }

        }

        void _midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {

            
        }


        public void Init()
        {
            BuildSignalChain();
            Debug.WriteLine("Latency: {0}", m_latency);
            _player = new WaveOutPlayer(m_latency);
            _player.PlaybackStopped += _player_PlaybackStopped;

            CanPlay = true;
            IsPlaying = false;
        }



        internal void PlayNote()
        {
            var note = PadSound.GetNote(new MidiNote(69, 440.0),127);
            HeadSampleProvider.AddMixerInput(note);
        }

        internal void StopNote()
        {
            HeadSampleProvider.RemoveAllMixerInputs();
        }
    }

}
