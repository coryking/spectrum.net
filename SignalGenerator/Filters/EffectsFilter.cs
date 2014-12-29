using CorySignalGenerator.Filters;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CorySignalGenerator.Filters
{
    public class EffectsFilter : INotifyPropertyChanged, ISampleProvider
    {
        private ISampleProvider _source;
        private ISampleProvider _headProvider;
        
        public EffectsFilter(ISampleProvider source, int outputChannels)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, outputChannels);
            _source = source;
            RebuildSignalChain();

            LowPassCutoff = 22000.0f;
            Q = 0.5f;
            ReverbDelay = 100;
            ReverbDecay = 0.2f;
            _types = new List<string>()
            {
                NAudio.Wave.SampleProviders.SignalGeneratorType.Pink.ToString(),
                NAudio.Wave.SampleProviders.SignalGeneratorType.SawTooth.ToString(),
                NAudio.Wave.SampleProviders.SignalGeneratorType.Sin.ToString(),
                NAudio.Wave.SampleProviders.SignalGeneratorType.Square.ToString(),
                NAudio.Wave.SampleProviders.SignalGeneratorType.Sweep.ToString(),
                NAudio.Wave.SampleProviders.SignalGeneratorType.Triangle.ToString(),
                NAudio.Wave.SampleProviders.SignalGeneratorType.White.ToString(),
            };

        }

        private FourPolesLowPassFilter lfoFilter;
        private GhettoReverb reverbFilter;

        private void RebuildSignalChain()
        {

            lfoFilter = new FourPolesLowPassFilter(_source);
           
            if (WaveFormat.Channels == 2 && _source.WaveFormat.Channels != WaveFormat.Channels)
            {
                _headProvider = new MonoToStereoSampleProvider(lfoFilter);
            }
            else
            {
                _headProvider = lfoFilter;
            }
            reverbFilter = new GhettoReverb(_headProvider);
            _headProvider = new DynamicConvolvingFilter(_headProvider);//reverbFilter;
        }

        private List<String> _types;

        public List<String> Types
        {
            get { return _types; }
        }

        public NAudio.Wave.SampleProviders.SignalGeneratorType Type { get; set; }

        public float ReverbDelay
        {
            get { return reverbFilter.Delay; }
            set
            {
                if (reverbFilter.Delay != value)
                {
                    reverbFilter.Delay = value;

                    OnPropertyChanged("ReverbDelay");
                }
            }
        }


        public float ReverbDecay
        {
            get { return reverbFilter.Decay; }
            set {
                if (reverbFilter.Decay != value)
                {
                    reverbFilter.Decay = value;
                    OnPropertyChanged("ReverbDecay");
                }

            }
        }


        public float LowPassCutoff
        {
            get { return lfoFilter.Frequency; }
            set
            {
                if (lfoFilter.Frequency != value)
                {
                    lfoFilter.Frequency = value; 
                    OnPropertyChanged("LowPassCutoff");
                }

            }
        }


        public float Q
        {
            get { return lfoFilter.Q; }
            set
            {
                if (lfoFilter.Q != value)
                {
                    lfoFilter.Q = value;
                    OnPropertyChanged("Q");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); 
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _headProvider.Read(buffer, offset, count);
        }

        public WaveFormat WaveFormat
        {
            get;
            private set;
        }
    }
}
