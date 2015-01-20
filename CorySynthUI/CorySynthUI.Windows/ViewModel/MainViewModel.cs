﻿using System;
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
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using NAudioWin8Demo;
using NAudio.Wave.SampleProviders;

namespace CorySynthUI.ViewModel
{
    public class MainViewModel : PropertyChangeModel
    {
        private MidiDeviceWatcher _watcher;
        private CoreDispatcher _dispatcher;
        private WindowsPreview.Devices.Midi.MidiInPort _midiIn;

        private readonly int m_latency = 50;// Convert.ToInt32(Math.Pow(2, 13) / (44100 / 1000)) - 1;

        public const int TicksPerBeat = 24;
        public const double BeatsPerMinute = 60;

        private ChannelSampleProvider _sampler;
        private WaveOutPlayer _player;


        public void GenerateWaveTable()
        {
            //StopPlaying();
            BuildWavetable();
        }

        #region Properties


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

        private bool _canPlay;
        public bool CanPlay
        {
            get { return _canPlay; }
            set { Set(ref _canPlay, value); }
        }

        private void SetCanPlay()
        {

            CanPlay = (SelectedModel.IsSampleTableLoaded && !_player.IsActive && !IsPlaying);
        }



        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { Set(ref _isPlaying, value); }
        }

        public Adsr Adsr { get; private set; }

        public PadSound PadSound { get; set; }

        public SignalGeneretedSound GeneratedSound { get; set; }

        
        #region Property Reverb
        private GhettoReverb _reverb = null;

        /// <summary>
        /// Sets and gets the Reverb property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GhettoReverb Reverb
        {
            get
            {
                return _reverb;
            }
            set
            {
                Set(ref _reverb, value);
            }
        }
        #endregion

        
        #region Property BandpassFilter
        private FourPolesLowPassFilter _bandpassFilter = null;

        /// <summary>
        /// Sets and gets the BandpassFilter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FourPolesLowPassFilter BandpassFilter
        {
            get
            {
                return _bandpassFilter;
            }
            set
            {
                Set(ref _bandpassFilter, value);
            }
        }
        #endregion
		

        #region Property Chorus
        private ChorusEffect _chorus = null;

        /// <summary>
        /// Sets and gets the Chorus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ChorusEffect Chorus
        {
            get
            {
                return _chorus;
            }
            set
            {
                Set(ref _chorus, value);
            }
        }
        #endregion
		

        public ISampleProvider MonoToStereo { get; private set; }

        #region Property SelectedModel
        private ISoundModel _selectedModel = null;

        /// <summary>
        /// Sets and gets the SelectedModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ISoundModel SelectedModel
        {
            get
            {
                return _selectedModel;
            }
            set
            {
                Set(ref _selectedModel, value);
                if (_sampler != null)
                    _sampler.NoteProvider = value;
            }
        }
        #endregion
		
        
        #region Property ModelTypes
        private ObservableCollection<ISoundModel> _modelTypes = new ObservableCollection<ISoundModel>();

        /// <summary>
        /// Sets and gets the ModelTypes property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ISoundModel> ModelTypes
        {
            get
            {
                return _modelTypes;
            }
            set
            {
                Set(ref _modelTypes, value);
            }
        }
        #endregion
		
        #endregion

        public MainViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        void _player_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            SetCanPlay();
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
            PadSound = new PadSound(baseWaveFormat)
            {
                Harmonics = 12,
                Bandwidth = 20,
                BandwidthScale = 1.0f,
                SampleSize = (int)Math.Pow(2, 15) * 2,//baseWaveFormat.SampleRate * 2,
            };
            GeneratedSound = new SignalGeneretedSound(baseWaveFormat);
            ModelTypes.Clear();
            ModelTypes.Add(PadSound);
            ModelTypes.Add(GeneratedSound);
            SelectedModel = GeneratedSound;
            BuildWavetable();
            _sampler = new ChannelSampleProvider(SelectedModel, Adsr);
            ISampleProvider pre_bandpass = _sampler;
            if (_sampler.WaveFormat.Channels != 2 )
            {
                pre_bandpass = new MonoToStereoSampleProvider(_sampler);
            }
            BandpassFilter = new FourPolesLowPassFilter(pre_bandpass);
            Chorus = new ChorusEffect(BandpassFilter);
            Reverb = new GhettoReverb(Chorus);
            HeadSampleProvider = Reverb;
            
        }

        private void BuildWavetable()
        {
            Task.Factory.StartNew(() =>
            {
                PadSound.InitSamples();
            }).ContinueWith((t) =>
            {
                _dispatcher.RunIdleAsync((e) =>
                {
                    SetCanPlay();
                });
            });
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

        private WaveStream GetWaveStream()
        {
            if (selectedStream != null)
                return new MediaFoundationReaderRT(selectedStream);
            else
                return null;
        }


        private IRandomAccessStream selectedStream;

        public async void LoadReverb()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            var stream = await file.OpenAsync(FileAccessMode.Read);
            if (stream == null) return;
            this.selectedStream = stream;
            SetCanPlay();
            //this.SetReverbFilter();
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

        public void StartListening(DeviceInformation device)
        {
            if (_midiIn == null)
            {
                var theTask = Task.Run(async () =>
                {
                    return await WindowsPreview.Devices.Midi.MidiInPort.FromIdAsync(device.Id);
                }).ContinueWith((task) =>
                {
                    if (task.IsFaulted)
                        throw task.Exception;

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
                    var msg = (WindowsPreview.Devices.Midi.MidiControlChangeMessage)args.Message;
                    _sampler.ControlChange(msg.Channel,msg.Controller,msg.ControlValue);
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
            Adsr = new Adsr();
            BuildSignalChain();
            Debug.WriteLine("Latency: {0}", m_latency);
            _player = new WaveOutPlayer(m_latency);
            _player.PlaybackStopped += _player_PlaybackStopped;
            MidiDevices = new ObservableCollection<DeviceInformation>();

            _watcher = new MidiDeviceWatcher();
            _watcher.MidiDevicesChanged += _watcher_MidiDevicesChanged;
            _watcher.Start();

            SetCanPlay();
            IsPlaying = false;
        }

        internal void ResetSounds()
        {
            _sampler.Reset();
            
        }
    }
}
