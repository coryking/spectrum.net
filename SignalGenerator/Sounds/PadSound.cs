using CorySignalGenerator.Dsp;
using CorySignalGenerator.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using System.Collections.Concurrent;
using CorySignalGenerator.Extensions;
using CorySignalGenerator.Models;
using System.Diagnostics;
using CorySignalGenerator.Filters;
using NAudio.Wave.SampleProviders;
using System.Collections.ObjectModel;
using CorySignalGenerator.Sequencer.Interfaces;
using CorySignalGenerator.Sequencer;
using CorySignalGenerator.Dsp.Harmonics;

namespace CorySignalGenerator.Sounds
{
    public class AmplitudeValue : PropertyChangeModel
    {
        public AmplitudeValue(int index)
        {
            Value = 0.5f;
            Index = index;
        }


        #region Property Value
        private float _value = 0.5f;

        /// <summary>
        /// Sets and gets the Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                Set(ref _value, value);
            }
        }
        #endregion
		

        #region Property Index
        private int _index = 1;

        /// <summary>
        /// Sets and gets the Index property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                Set(ref _index, value);
            }
        }
        #endregion
		


    }

    public class PadSound : NoteSampler
    {

        //private readonly int[] notesToSample = new int[]{
        //    9, // octave -1
        //    21, 23, // octave 0
        //    24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, // octave 1
        //    36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, // octave 2
        //    48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, // octave 3
        //    60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, // octave 4
        //    72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, // octave 5
        //    84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, // octave 6
        //    96, 97, 98, 101, 105, // octave 7
        //    108, 116, // octave 8
        //    120, 127 // octave 9
        //};
        private readonly int[] notesToSample = new int[]{
            9, // octave -1
            21, 23, // octave 0
            24, 25, 27, 31, 35, // octave 1
            36, 38, 40, 41, 43, 45, 47, // octave 2
            48, 50, 52, 53, 55, 57, 59, // octave 3
            60, 62, 64, 65, 67, 69, 71, // octave 4
            72, 74, 76, 77, 79, 81, 83, // octave 5
            84, 86, 88, 89, 91, 93, 95, // octave 6
            96, 98, 101, 105, // octave 7
            108, 116, // octave 8
            120, 127 // octave 9
        };
        #region Properties
        public override string Name { get { return "Pad Sound"; } }

        public RelayCommand BuildWaveTableCommand { get; set; }

        /// <summary>
        /// Wave lookup table.  Each key is a midi note number
        /// </summary>
        protected ConcurrentDictionary<int, SampleSource> WaveTable
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of harmonics (eg: 10)
        /// </summary>

        #region Property Harmonics
        private int _harmonics = 24;

        /// <summary>
        /// Sets and gets the Harmonics property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Harmonics
        {
            get
            {
                return _harmonics;
            }
            set
            {
                Set(ref _harmonics, value);
                SetupAmplitudes();
            }
        }
        #endregion


        #region Property HarmonicOffsetMaker
        private IHarmonicPosition _harmonicOffsetMaker = null;

        /// <summary>
        /// Sets and gets the HarmonicOffsetMaker property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IHarmonicPosition HarmonicOffsetMaker
        {
            get
            {
                return _harmonicOffsetMaker;
            }
            set
            {
                Set(ref _harmonicOffsetMaker, value);
            }
        }
        #endregion

        private List<IHarmonicPosition> _harmonicOffsetMakerList = new List<IHarmonicPosition>()
        {
            new LinearHarmonic(),
            new PowerHarmonicPosition()
        };

        public List<IHarmonicPosition> HarmonicOffsetMakerList
        {
            get { return _harmonicOffsetMakerList; }
        }


        #region Property InterpolateAmplitudes
        private bool _interpolateAmplitudes = false;

        /// <summary>
        /// Gets or Sets if the amlitude values should be interpolated
        /// </summary>
        public bool InterpolateAmplitudes
        {
            get
            {
                return _interpolateAmplitudes;
            }
            set
            {
                Set(ref _interpolateAmplitudes, value);
            }
        }
        #endregion
		


        /// <summary>
        /// bandwidth in cents of the fundamental frequency (eg. 25 cents)
        /// </summary>
        public float Bandwidth { get; set; }

        /// <summary>
        /// how the bandwidth increase on the higher harmonics (recomanded value: 1.0)
        /// </summary>
        public float BandwidthScale { get; set; }

        /// <summary>
        /// Gets the size of the sample (must be a power of two)
        /// </summary>
        public int SampleSize { get; set; }


        public ObservableCollection<AmplitudeValue> Amplitudes { get; set; }


        public override NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            protected set;
        }


        public bool IsSampleTableLoaded
        {
            get;
            private set;
        }

#endregion

        protected void SetupAmplitudes()
        {
            if (Amplitudes == null)
                return;

            while (Amplitudes.Count > Harmonics)
            {
                Amplitudes.RemoveAt(Amplitudes.Count - 1);
            }
            while (Amplitudes.Count < Harmonics)
            {
                var i = Amplitudes.Count;
                var value = 1f;
                if (i > 0)
                    value = Amplitudes[i - 1].Value * 0.5f;

                var index = i + 1;
                Amplitudes.Add(new AmplitudeValue(index) { Value = value });
            }

        }

 
        public PadSound(WaveFormat waveFormat)
        {
            Bandwidth = 25f;
            BandwidthScale = 1.0f;
            Amplitudes = new ObservableCollection<AmplitudeValue>();

            Harmonics = 24;
            WaveFormat = waveFormat;
            SampleSize = (int)Math.Pow(2, 16);
            WaveTable = new ConcurrentDictionary<int, SampleSource>();
            BuildWaveTableCommand = new RelayCommand(BuildWaveTableCommandExecute);
        }

        /// <summary>
        /// Method that gets run when somebody executes the <see cref="BuildWaveTableCommand"/>
        /// </summary>
        /// <param name="parameter"></param>
        protected void BuildWaveTableCommandExecute(object parameter)
        {
            Task.Run(() =>
            {
                InitSamples();
            });
        }


        public void InitSamples()
        {
            if (WaveTable == null || WaveTable.Count == 0)
                IsSampleTableLoaded = false;

            var allNotes = MidiNotes.GenerateNotes();
            var notesToGen = new List<MidiNote>();
            //var notesToSample = new int[] { 60 };
            for (var i = 0; i < notesToSample.Length; i++)
            {
                var notenumber = notesToSample[i];
                notesToGen.Add(allNotes[notenumber]);
            }

            Debug.WriteLine("Min Freq: {0}, Max Freq: {1}", notesToGen.Min(x=>x.Frequency), notesToGen.Max(x=>x.Frequency));
            //var freqs = new float[] { 440f };
            var amplitude_values = Amplitudes.Select((x) => { return x.Value; }).ToArray();
            var newWaveTable = new ConcurrentDictionary<int, SampleSource>();
            var harmonicOffsetMaker = (HarmonicOffsetMaker == null) ? new LinearHarmonic() : HarmonicOffsetMaker;

            Parallel.ForEach(notesToGen, (note) =>
            {
                var amplitudes = amplitude_values;
                if(InterpolateAmplitudes)
                {
                    var rescaleLength = (float)Math.Max(4, 440f / note.Frequency);
                    amplitudes = Dsp.LinearInterpolator.Rescale(amplitude_values, rescaleLength);
            
                }

                var sample =
                     LegacyPADsynth.GenerateWaveTable(amplitudes, (float)note.Frequency, Bandwidth, BandwidthScale, harmonicOffsetMaker, note.Number, SampleSize, WaveFormat.SampleRate, WaveFormat.Channels);
                newWaveTable.AddOrUpdate(note.Number, sample, (key, value) => sample);

            });
            WaveTable = newWaveTable;
            IsSampleTableLoaded = true;
        }


        protected override ISampleProvider GenerateNote(MidiNote note)
        {
            if (!IsSampleTableLoaded)
                throw new InvalidOperationException("Cannot get a provider.  No wave table has been created");
            //var largerValues = WaveTable.Values.Where(x => (frequency <= x.FundamentalFrequency));//.MinBy(x => x.FundamentalFrequency);//.MinBy(x => Math.Abs(x.FundamentalFrequency - frequency));
            //var nearestNote = largerValues.MinBy(x => x.FundamentalFrequency);
            var nearestNote = WaveTable.Values.MinBy(x => Math.Abs(x.FundamentalFrequency - note.Frequency));
            var music_sampler = new MusicSampleProvider(WaveTable[nearestNote.Note]);
            ISampleProvider outputProvider;
            var noteDelta = note.Number - nearestNote.Note;
            if (noteDelta != 0)
            {
                var windowFactor = 4 + 4 * note.Number / 128;
                var windowSize = 50f; //windowFactor * 1000 * 1 / nearestNote.FundamentalFrequency;
                var overlapSize = windowSize * 2 / 5f;
                Debug.WriteLine("Shift {0} to {1}. w: {2}, o: {3}", note, nearestNote, windowSize, overlapSize);

                outputProvider = new SuperPitch(music_sampler)
                {
                    PitchOctaves = 0f,
                    PitchSemitones = noteDelta,
                    WindowSize = windowSize,
                    OverlapSize = overlapSize
                };
            }
            else
            {
                outputProvider = music_sampler;
            }
            return outputProvider;
        }

        protected override bool SupportsVelocity
        {
            get { return true; }
        }
    }
}
