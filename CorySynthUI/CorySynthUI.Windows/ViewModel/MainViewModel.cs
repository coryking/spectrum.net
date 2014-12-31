using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using WindowsPreview.Devices.Midi;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using System.Diagnostics;
using CorySignalGenerator.Models;
using CorySignalGenerator.Sequencer;
using CorySignalGenerator.Filters;
using CorySignalGenerator.Wave;
using CorySignalGenerator.Sounds;
using NAudio.Wave;

namespace CorySynthUI.ViewModel
{
    public class MainViewModel : PropertyChangeModel
    {
        private MidiDeviceWatcher _watcher;
        private CoreDispatcher _dispatcher;
        private WindowsPreview.Devices.Midi.MidiInPort _midiIn;

        public const int TicksPerBeat = 24;
        public const double BeatsPerMinute = 60;

        private ISoundModel _noteModel;
        private ChannelSampleProvider _sampler;
        private EffectsFilter _effects;
        private WaveOutPlayer _player;

        public ISampleProvider HeadSampleProvider
        {
            get;
            private set;
        }

        public double TicksPerMinute
        {
            get { return TicksPerBeat * BeatsPerMinute; }
        }

        /// <summary>
        /// Gets the number of ticks per millisecond
        /// </summary>
        public double TicksPerMs
        {
            get { return TicksPerMinute / (60.0 * 1000.0); }
        }

        public void GenerateWaveTable()
        {
            StopPlaying();
            _noteModel.InitSamples();
        }

        public PadSound PadSound { get { return _noteModel as PadSound; } }

        public EffectsFilter EffectsFilter { get { return _effects; } }

        public MainViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

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
                Harmonics = 64,
                Bandwidth = 20,
                BandwidthScale = 1.0f,
                SampleSize = (int)Math.Pow(2, 15) * 2,//baseWaveFormat.SampleRate * 2,
                AttackSeconds = 0.5f,
                ReleaseSeconds = 0.5f
            };
            _noteModel.InitSamples();
            _sampler = new ChannelSampleProvider(_noteModel);
            _effects = new EffectsFilter(_sampler, 2)
            {
                ReverbDecay = 0.5f,
                ReverbDelay = 0.25f
            };
            HeadSampleProvider = _effects;
        }

        void _watcher_MidiDevicesChanged(MidiDeviceWatcher sender)
        {

            var items = sender.GetDeviceInformationCollection().Where(x => x.IsEnabled).OrderBy(x => x.Name);
            _dispatcher.RunIdleAsync((e) =>
            {
                MidiDevices.Clear();
                foreach (var item in items)
                {
                    MidiDevices.Add(item);

                }
            });
           
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
                _midiIn.MessageReceived -= _midiIn_MessageReceived;
                _midiIn.Dispose();
                _midiIn = null;
            }
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

        public void StartListening(DeviceInformation device)
        {
            if (_midiIn == null)
            {
                var theTask = Task.Run(async () =>
                {
                    return await WindowsPreview.Devices.Midi.MidiInPort.FromIdAsync(device.Id);
                }).ContinueWith((task) =>
                {
                    _midiIn = task.Result;
                    if (_midiIn == null)
                        throw new InvalidOperationException("Could not get midi device");
                    _midiIn.MessageReceived += _midiIn_MessageReceived;
                });
            }

        }

        void _midiIn_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            if (_sampler == null || !_player.IsActive)
                return;

            switch (args.Message.Type)
            {
                case MidiMessageType.ActiveSensing:
                    break;
                case MidiMessageType.ChannelPressure:
                    break;
                case MidiMessageType.Continue:
                    break;
                case MidiMessageType.ControlChange:
                    break;
                case MidiMessageType.MidiTimeCode:
                    break;
                case MidiMessageType.None:
                    break;
                case MidiMessageType.NoteOff:
                    var offNote = (WindowsPreview.Devices.Midi.MidiNoteOffMessage)args.Message;
                    _sampler.StopNote(offNote.Note);
                    break;
                case MidiMessageType.NoteOn:
                    var onNote=(WindowsPreview.Devices.Midi.MidiNoteOnMessage)args.Message;
                    if (onNote.Velocity > 0)
                        _sampler.PlayNote(onNote.Note, onNote.Velocity);
                    else
                        _sampler.StopNote(onNote.Note);
                    break;
                case MidiMessageType.PitchBendChange:
                    break;
                case MidiMessageType.PolyphonicKeyPressure:
                    break;
                case MidiMessageType.ProgramChange:
                    break;
                case MidiMessageType.SongPositionPointer:
                    break;
                case MidiMessageType.SongSelect:
                    break;
                case MidiMessageType.Start:
                    break;
                case MidiMessageType.Stop:
                    break;
                case MidiMessageType.SystemExclusive:
                    break;
                case MidiMessageType.SystemReset:
                    break;
                case MidiMessageType.TimingClock:
                    break;
                case MidiMessageType.TuneRequest:
                    break;
                default:
                    break;
            }
        }

        public ObservableCollection<DeviceInformation> MidiDevices { get; private set; }


        public void Init()
        {
            BuildSignalChain();
            _player = new WaveOutPlayer();
            _player.PlaybackStopped += _player_PlaybackStopped;
            MidiDevices = new ObservableCollection<DeviceInformation>();

            _watcher = new MidiDeviceWatcher();
            _watcher.MidiDevicesChanged += _watcher_MidiDevicesChanged;
            _watcher.Start();

            CanPlay = true;
            IsPlaying = false;
        }
    }
}
