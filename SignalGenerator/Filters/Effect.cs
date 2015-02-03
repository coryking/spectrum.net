using CorySignalGenerator.Models;
using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public abstract class Effect : PropertyChangeModel, ISampleProvider, IEffect
    {
        protected float min(float a, float b) { return Math.Min(a, b); }
        protected float max(float a, float b) { return Math.Max(a, b); }
        protected float abs(float a) { return Math.Abs(a); }
        protected float exp(float a) { return (float)Math.Exp(a); }
        protected float sqrt(float a) { return (float)Math.Sqrt(a); }
        protected float sin(float a) { return (float)Math.Sin(a); }
        protected float tan(float a) { return (float)Math.Tan(a); }
        protected float cos(float a) { return (float)Math.Cos(a); }
        protected float pow(float a, float b) { return (float)Math.Pow(a, b); }
        protected float sign(float a) { return Math.Sign(a); }
        protected float log(float a) { return (float)Math.Log(a); }
        protected float PI { get { return (float)Math.PI; } }

        public Effect(WaveFormat format)
        {
            setupVars(format, null);
        }

        public Effect(ISampleProvider sourceProvider)
        {
            setupVars(sourceProvider.WaveFormat, sourceProvider);
        }
        private void setupVars(WaveFormat format, ISampleProvider sourceProvider)
        {
            if (RequiresStereo && format.Channels != 2)
                throw new InvalidOperationException(String.Format("Effect {0} requires stereo", this.Name));
            WaveFormat = format;
            Source = sourceProvider;
            Init();
        }

        /// <summary>
        /// Initalize this filter
        /// </summary>
        protected virtual void Init() { }

        public int Read(float[] buffer, int offset, int count)
        {
            if (IsEnabled)
                return OnRead(buffer, offset, count);

            if (Source != null)
                return Source.Read(buffer, offset, count);

            return 0;
        }

        /// <summary>
        /// The meat of this filter.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected abstract int OnRead(float[] buffer, int offset, int count);

        // Reset the filter
        protected virtual void Reset()
        {
            // Do nothing
        }
        #region Property IsEnabled
        private bool _isEnabled = true;

        /// <summary>
        /// Sets and gets the IsEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                Set(ref _isEnabled, value);
                Reset();
            }
        }
        #endregion

        /// <summary>
        /// The source for this effect
        /// </summary>
        public ISampleProvider Source
        {
            get;
            set;
        }

        public WaveFormat WaveFormat
        {
            get;
            protected set;
        }

        protected int Channels
        {
            get { return WaveFormat.Channels; }
        }

        protected int SampleRate
        {
            get { return WaveFormat.SampleRate; }
        }

        protected virtual bool RequiresStereo
        {
            get { return false; }
        }

        public virtual int Order
        {
            get { return 50; }
        }

        public abstract string Name
        {
            get;
        }

    }
}
