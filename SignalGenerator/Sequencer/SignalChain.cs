using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer
{
    public class SignalChain<T> : ObservableCollection<T>, ISourcedSampleProvider where T : ISourcedSampleProvider
    {
        private object _lock = new object();
        
        public SignalChain(ISampleProvider source)
        {
            Source = source;
            Output = Source;
        }

        private ISampleProvider _source;
        /// <summary>
        /// Beginning of the signal chain
        /// </summary>
        public ISampleProvider Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (_source == value)
                    return;
                _source = value;
                RebuildEffectsChain();
                OnPropertyChanged("Source");
            }
        }


        /// <summary>
        /// The very last item in the signal chain
        /// </summary>
        protected ISampleProvider Output { get; private set; }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }


        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RebuildEffectsChain();
            base.OnCollectionChanged(e);
        }
        

        protected void RebuildEffectsChain()
        {
            lock (_lock)
            {
                ISampleProvider lastSource = Source;
                foreach (var effect in this)
                {
                    // Sanity check... everything must have the same wave format.
                    if (effect.WaveFormat != lastSource.WaveFormat)
                        throw new InvalidOperationException(String.Format("Sample providers do not share same WaveFormat {0}, {1}", lastSource, effect));

                    effect.Source = lastSource;
                    lastSource = effect;
                }
                Output = lastSource;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                // Sanity check
                Debug.Assert(Output != null);
                if (Output == null)
                    return 0;

                return Output.Read(buffer, offset, count);
            }
        }

        public WaveFormat WaveFormat
        {
            get { return Source.WaveFormat; }
        }
    }
}
