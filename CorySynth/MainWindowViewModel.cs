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



        private readonly int m_latency = 15; // Convert.ToInt32(Math.Pow(2, 13) / (44100 / 1000)) - 1;

        public const int TicksPerBeat = 24;
        public const double BeatsPerMinute = 60;

        private ISoundModel _noteModel;
        private ChannelSampleProvider _sampler;
        private EffectsFilter _effects;
        private ReverbFilter _reverb;
        private WaveOutPlayer _player;
        private Dispatcher _dispatcher;
        private NAudio.Midi.MidiIn _midiIn;
        

        private String reverbFile;

        public void GenerateWaveTable()
        {
            StopPlaying();
            _noteModel.InitSamples();
        }

        #region Properties

        
        #region Property Adsr
        private Adsr _adsr = new Adsr();

        /// <summary>
        /// Sets and gets the Adsr property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Adsr Adsr
        {
            get
            {
                return _adsr;
            }
            set
            {
                Set(ref _adsr, value);
            }
        }
        #endregion
		


        public ISampleProvider HeadSampleProvider
        {
            get;
            private set;
        }

        public double TicksPerMinute
        {
            get { return TicksPerBeat * BeatsPerMinute; }
        }


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

        public List<MidiInCapabilities> MidiDevices { get; private set; }

        /// <summary>
        /// Gets the number of ticks per millisecond
        /// </summary>
        public double TicksPerMs
        {
            get { return TicksPerMinute / (60.0 * 1000.0); }
        }
        public PadSound PadSound { get { return _noteModel as PadSound; } }

        public EffectsFilter EffectsFilter { get { return _effects; } }

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
            HeadSampleProvider = null;
            var baseWaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            //_noteModel = new SignalGeneretedSound(NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100,1))
            //{
            //    AttackSeconds=0.5f,
            //    ReleaseSeconds=0.5f,
            //    Type=NAudio.Wave.SampleProviders.SignalGeneratorType.Square,
            //};
            _noteModel = new PadSound(baseWaveFormat)
            {
                Harmonics = 12,
                Bandwidth = 20,
                BandwidthScale = 1.0f,
                SampleSize = (int)Math.Pow(2, 15) * 2,//baseWaveFormat.SampleRate * 2,
            };
            _noteModel.InitSamples();
            _sampler = new ChannelSampleProvider(_noteModel, Adsr);
            _effects = new EffectsFilter(_sampler, 2);
            _effects.GhettoReverbFilter.Delay = 0.25f;
            _effects.GhettoReverbFilter.Decay = 0.5f;
            HeadSampleProvider = _effects;

        }

        private void SetReverbFilter()
        {
            if (!String.IsNullOrEmpty(reverbFile))
            {
                //var renderSliceSize = m_latency * 44100/1000;
                using (var stream = new AudioFileReader(reverbFile))
                {
                    _reverb = new ReverbFilter(_effects, MaxFFTSize, true);
                    _reverb.LoadImpuseResponseWaveStream(stream);
                }
                HeadSampleProvider = _reverb;
            }
            else
            {
                HeadSampleProvider = _effects;
                _reverb = null;
            }

        }


        public void LoadReverb()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.wav";
            dlg.Filter = "Wave Files (.wav)|*.wav";
            var result = dlg.ShowDialog();
            if (result == true)
            {
                CanPlay = true;
                reverbFile = dlg.FileName;
                this.SetReverbFilter();
            }
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

            if (_sampler == null || !_player.IsActive)
                return;

            switch (e.MidiEvent.CommandCode)
            {
                case MidiCommandCode.AutoSensing:
                    break;
                case MidiCommandCode.ChannelAfterTouch:
                    break;
                case MidiCommandCode.ContinueSequence:
                    break;
                case MidiCommandCode.ControlChange:
                    break;
                case MidiCommandCode.Eox:
                    break;
                case MidiCommandCode.KeyAfterTouch:
                    break;
                case MidiCommandCode.MetaEvent:
                    break;
                case MidiCommandCode.NoteOff:
                    var offNote = (NAudio.Midi.NoteEvent)e.MidiEvent;
                    _sampler.StopNote(offNote.NoteNumber);
                    break;
                case MidiCommandCode.NoteOn:
                    var onNote = (NAudio.Midi.NoteOnEvent)e.MidiEvent;
                    if (onNote.Velocity > 0)
                        _sampler.PlayNote(onNote.NoteNumber, onNote.Velocity);
                    else
                        _sampler.StopNote(onNote.NoteNumber);
                    break;
                case MidiCommandCode.PatchChange:
                    break;
                case MidiCommandCode.PitchWheelChange:
                    break;
                case MidiCommandCode.StartSequence:
                    break;
                case MidiCommandCode.StopSequence:
                    break;
                case MidiCommandCode.Sysex:
                    break;
                case MidiCommandCode.TimingClock:
                    break;
                default:
                    break;
            }
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


    }

}
