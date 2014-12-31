using CorySignalGenerator.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public abstract class Effect : PropertyChangeModel, ISampleProvider
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


        public Effect(ISampleProvider sourceProvider)
        {
            Source = sourceProvider;
            WaveFormat = Source.WaveFormat;
            Init();
        }

        /// <summary>
        /// Initalize this filter
        /// </summary>
        protected virtual void Init() { }


        /// <summary>
        /// The meat of this filter.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public abstract int Read(float[] buffer, int offset, int count);
        
        public ISampleProvider Source
        {
            get;
            protected set;
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
    }
}
