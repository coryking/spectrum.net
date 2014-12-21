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
namespace CorySynth
{
    public class MainWindowViewModel
    {
        private MixingSampleProvider _mixer;
        private Timer timer;
        public const int TicksPerBeat = 24;
        public const double BeatsPerMinute = 60;

        public double TicksPerMinute
        {
            get { return TicksPerBeat * BeatsPerMinute; }
        }

        /// <summary>
        /// Gets the number of ticks per millisecond
        /// </summary>
        public double TicksPerMs
        {
            get { return TicksPerMinute / (60.0 * 1000.0);  }
        }

        public MainWindowViewModel()
        {
            Channels = AudioChannels.MONO;

            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, (int)Channels)); ;
            _mixer.ReadFully = true;
            Adsr = new Models.Adsr();

            this.MidiDevices = new List<MidiInCapabilities>();
            for (var i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                MidiDevices.Add(MidiIn.DeviceInfo(i));
            }

            Sequence = new MidiEventCollection(0, TicksPerBeat);
            AddSimpleNote(72, 4);
            AddSimpleNote(76, 4);
            AddSimpleNote(79, 4);
            AddSimpleNote(84, 4);
            AddStartNote(72);
            AddStartNote(76, 1);
            AddStartNote(79, 1);
            AddStartNote(84, 1);
            AddEndNote(72, 1);
            AddEndNote(76);
            AddEndNote(79);
            AddEndNote(84);

            //Sequence.AddEvent(new NoteOnEvent(0, 10, 60, 100, 15), 10);
            //Sequence.AddEvent(new NoteOnEvent(15, 10, 60, 100, 15), 10);
            //Sequence.AddEvent(new NoteOnEvent(30, 10, 60, 100, 15), 10);
            SequencePlayer = new Player(Sequence);
            SequencePlayer.NewMidiEvents += SequencePlayer_NewMidiEvents;
            SequencePlayer.SequenceEnded += SequencePlayer_SequenceEnded;
            Tracker = new NoteTracker();

            timer = new Timer();
            timer.Interval = (1.0 / (TicksPerMinute)) * 60.0 * 1000.0;
            timer.Elapsed += timer_Elapsed;
            Console.WriteLine("Timer Interval: {0}", timer.Interval); 
            //timer.Interval = TimeSpan.FromSeconds(1 / (100 * Sequence.DeltaTicksPerQuarterNote));
            //timer.Tick += timer_Tick;
        }

        public List<MidiInCapabilities> MidiDevices { get; private set; }

        private void AddSimpleNote(int noteNumber, int beats, int deltaBeats=0)
        {
            Sequence.AddEvent(new NoteOnEvent(TicksPerBeat * deltaBeats, 1, noteNumber,100, 15), 1);
            Sequence.AddEvent(new NoteEvent(TicksPerBeat * beats, 1, MidiCommandCode.NoteOff,noteNumber, 0), 1);
            
        }

        private void AddStartNote(int noteNumber, int deltaBeats = 0)
        {
            Sequence.AddEvent(new NoteOnEvent(deltaBeats * TicksPerBeat, 1, noteNumber, 100, 15), 1);

        }
        private void AddEndNote(int noteNumber, int deltaBeats = 0)
        {
            Sequence.AddEvent(new NoteEvent(TicksPerBeat * deltaBeats, 1, MidiCommandCode.NoteOff, noteNumber, 0), 1);

        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            NextTick(); ;
        }

        public void Start()
        {
            SequencePlayer.Reset();
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }

        void SequencePlayer_SequenceEnded(Player sender)
        {
            timer.Stop();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            NextTick();
        }

        void SequencePlayer_NewMidiEvents(Player sender, Queue<MidiEvent> newEvents)
        {
            while (newEvents != null && newEvents.Count > 0)
            {
                var midiEvent = newEvents.Dequeue();
                HandleMidiEvent(midiEvent);


            }
        }

        private void HandleMidiEvent(MidiEvent midiEvent)
        {
            switch (midiEvent.CommandCode)
            {
                case MidiCommandCode.NoteOn:
                    if (((NoteEvent)midiEvent).Velocity > 0)
                        AddNewNote((NoteOnEvent)midiEvent);
                    else
                        StopNote((NoteEvent)midiEvent);
                    break;
                case MidiCommandCode.NoteOff:
                    StopNote((NoteEvent)midiEvent);
                    break;
                default:
                    break;
            }
        }

        private void StopNote(NoteEvent noteEvent)
        {
            var provider = Tracker.StopNote(noteEvent.NoteNumber);
            if (provider is AdsrSampleProvider)
            {
                ((AdsrSampleProvider)provider).Stop();
            }
        }

        private void AddNewNote(NoteOnEvent noteOnEvent)
        {
            var provider = Tracker.PlayNote(noteOnEvent.NoteNumber, (freq) =>
            {
                var wave = new SignalGenerator(SampleRate, (int)Channels)
                {
                    Frequency = freq,
                    Type = SignalGeneratorType.SawTooth
                };
                return new AdsrSampleProvider(wave)
                {
                    ReleaseSeconds=Adsr.ReleaseSeconds,
                    AttackSeconds=Adsr.AttackSeconds
                };
            });
            _mixer.AddMixerInput(provider);

        }

        public MidiEventCollection Sequence { get; set; }

        public Models.Adsr Adsr { get; set; }

        public Player SequencePlayer { get; set; }

        public NoteTracker Tracker { get; set; }
        
        public int SampleRate { get { return 44100; } }

        public AudioChannels Channels { get; set; }

        public ISampleProvider GetAudioChain()
        {
            return _mixer;
        }



        public void NextTick()
        {
            SequencePlayer.NextTick();
        }

        internal void PlayNote(MidiEvent midiEvent)
        {
            HandleMidiEvent(midiEvent);
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
