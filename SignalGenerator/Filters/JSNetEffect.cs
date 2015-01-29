using CorySignalGenerator.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace CorySignalGenerator.Filters
{
    public abstract class JSNetEffect: Effect, CorySignalGenerator.SampleProviders.IStoppableSample
    {
        protected Boolean isDirty;
        private object locker = new object();

        public JSNetEffect(WaveFormat format) : base(format) { }

        public JSNetEffect(ISampleProvider source) : base(source)
        {
        }

        protected override void Init()
        {
            base.Init();
            Effect = CreateEffectInstance.Invoke();
            Effect.Init();
            isDirty = true;
            
        }

        /// <summary>
        /// Create a new instance of the effect
        /// </summary>
        /// <returns></returns>
        protected abstract Func<JSNet.Effect> CreateEffectInstance { get; }


        protected JSNet.Effect Effect { get; set; }

        protected override void HandlePropertyChanged(string propertyName)
        {
            isDirty = true;
        }

        /// <summary>
        /// Take the C# properties and turn them into effect sliders, or whatever...
        /// </summary>
        protected abstract void SetEffectParams();

        protected void SetEffectSlider(int index, float value)
        {
            Effect.Sliders[index].Value = value;
        }

        private void UpdateParams()
        {

            if (!isDirty)
                return;

            lock (locker)
            {
                SetEffectParams();
                this.Effect.Slider();
                isDirty = false;
            }
            
        }

        protected override int OnRead(float[] buffer, int offset, int count)
        {
            var samplesRead = Source.Read(buffer, offset, count);

            UpdateParams();

            for (var i = 0; i < samplesRead; i += Channels)
            {
                var spl0 = buffer[offset];
                var spl1 = (Channels == 2) ? buffer[offset + 1] : 0f;
                Effect.Sample(ref spl0, ref spl1);
                buffer[offset++] = spl0;
                if (Channels == 2)
                    buffer[offset++] = spl1;
            }
            return samplesRead;
        }

        protected override void Reset()
        {
            base.Reset();
            Effect.Reset();
        }


        public void Stop()
        {
            if (this.Source is SampleProviders.IStoppableSample)
                ((SampleProviders.IStoppableSample)this.Source).Stop();
        }

    }
}
