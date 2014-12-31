// .NET port of Super-pitch JS effect included with Cockos REAPER
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public class SuperPitch : Effect
    {
        int bufsize;
        float xfade;
        int bufloc0;
        int bufloc1;
        int buffer0;
        int buffer1;
        int bufdiff;
        float pitch;

        float denorm;
        bool filter;
        float v0;
        float h01, h02, h03, h04;
        float h11, h12, h13, h14;
        float a1, a2, a3, b1, b2;
        float t0, t1;
        float drymix;
        float wetmix;
        float[] buffer = new float[64000];

        bool isDirty = false;


        public SuperPitch(ISampleProvider source) :base(source)
        {
        
        }
        
        protected override void Init()
        {
            bufsize = (int)SampleRate; // srate|0;
            xfade = 100;
            bufloc0 = 10000;
            bufloc1 = bufloc0 + bufsize + 1000;

            buffer0 = bufloc0;
            buffer1 = bufloc1;
            bufdiff = bufloc1 - bufloc0;
            pitch = 1.0f;
            denorm = pow(10, -20);
            isDirty = true;
            base.Init();

        }

        protected override void HandlePropertyChanged(string propertyName)
        {
            filter = true; // enable the filter....
            isDirty = true;
        }

        private void BuildoutParams()
        {
            int bsnew = (int)(Math.Min(WindowSize, 1000) * 0.001 * SampleRate);
            if (bsnew != bufsize)
            {
                bufsize = bsnew;
                v0 = buffer0 + bufsize * 0.5f;
                if (v0 > bufloc0 + bufsize)
                {
                    v0 -= bufsize;
                }
            }

            xfade = (int)(OverlapSize * 0.001 * SampleRate);
            if (xfade > bsnew * 0.5)
            {
                xfade = bsnew * 0.5f;
            }

            float npitch = pow(2, ((PitchSemitones + PitchCents * 0.01f) / 12 + PitchOctaves));
            if (pitch != npitch)
            {
                pitch = npitch;
                float lppos = (pitch > 1.0f) ? 1.0f / pitch : pitch;
                if (lppos < (0.1f / SampleRate))
                {
                    lppos = 0.1f / SampleRate;
                }
                float r = 1.0f;
                float c = 1.0f / tan(PI * lppos * 0.5f);
                a1 = 1.0f / (1.0f + r * c + c * c);
                a2 = 2 * a1;
                a3 = a1;
                b1 = 2.0f * (1.0f - c * c) * a1;
                b2 = (1.0f - r * c + c * c) * a1;
                h01 = h02 = h03 = h04 = 0;
                h11 = h12 = h13 = h14 = 0;
            }

            drymix = pow(2, (DryMix / 6));
            wetmix = pow(2, (WetMix / 6));
            isDirty = false;
        }

        #region properties

        #region Property PitchCents
        private float _pitchCents = 0.0f;

        /// <summary>
        /// Pitch Adjust (cents)
        /// </summary>
        public float PitchCents
        {
            get
            {
                return _pitchCents;
            }
            set
            {
                Set(ref _pitchCents, value);
            }
        }
        #endregion


        #region Property PitchSemitones
        private float _pitchSemitones = 0.0f;

        /// <summary>
        /// Pitch Adjust (semitones)
        /// </summary>
        public float PitchSemitones
        {
            get
            {
                return _pitchSemitones;
            }
            set
            {
                Set(ref _pitchSemitones, value);
            }
        }
        #endregion


        #region Property PitchOctaves
        private float _pitchOctaves = 0f;

        /// <summary>
        /// Pitch Adjust (octaves)
        /// </summary>
        public float PitchOctaves
        {
            get
            {
                return _pitchOctaves;
            }
            set
            {
                Set(ref _pitchOctaves, value);
            }
        }
        #endregion


        #region Property WindowSize
        private float _windowSize = 50f;

        /// <summary>
        /// Window Size (ms)
        /// </summary>
        public float WindowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                Set(ref _windowSize, value);
            }
        }
        #endregion


        #region Property OverlapSize
        private float _overlapSize = 20f;

        /// <summary>
        /// Overlap Size (ms)
        /// </summary>
        public float OverlapSize
        {
            get
            {
                return _overlapSize;
            }
            set
            {
                Set(ref _overlapSize, value);
            }
        }
        #endregion


        #region Property WetMix
        private float _wetMix = 0f;

        /// <summary>
        /// Wet Mix (dB)
        /// </summary>
        public float WetMix
        {
            get
            {
                return _wetMix;
            }
            set
            {
                Set(ref _wetMix, value);
            }
        }
        #endregion


        #region Property DryMix
        private float _dryMix = -120f;

        /// <summary>
        /// Dry Mix (dB)
        /// </summary>
        public float DryMix
        {
            get
            {
                return _dryMix;
            }
            set
            {
                Set(ref _dryMix, value);
            }
        }
        #endregion

        #endregion

        public override int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = Source.Read(buffer, offset, count);
            if (!filter)
                return samplesRead;

            if (isDirty)
                BuildoutParams();

            for (var i = 0; i < samplesRead; i += Channels)
            {
                var spl0 = buffer[i];
                var spl1 = (Channels == 2) ? buffer[i + 1] : 0f;
                Sample(ref spl0, ref spl1);
                buffer[i] = spl0;
                if (Channels == 2)
                    buffer[i + 1] = spl1;
            }
            return samplesRead;
        }

        /// <summary>
        /// Compute a sample
        /// </summary>
        /// <param name="spl0">left channel</param>
        /// <param name="spl1">right channel</param>
        protected void Sample(ref float spl0, ref float spl1)
        {
            int iv0 = (int)(v0);
            float frac0 = v0 - iv0;
            int iv02 = (iv0 >= (bufloc0 + bufsize - 1)) ? iv0 - bufsize + 1 : iv0 + 1;

            float ren0 = (buffer[iv0 + 0] * (1 - frac0) + buffer[iv02 + 0] * frac0);
            float ren1 = (buffer[iv0 + bufdiff] * (1 - frac0) + buffer[iv02 + bufdiff] * frac0);
            float vr = pitch;
            float tv, frac, tmp, tmp2;
            if (vr >= 1.0)
            {
                tv = v0;
                if (tv > buffer0) tv -= bufsize;
                if (tv >= buffer0 - xfade && tv < buffer0)
                {
                    // xfade
                    frac = (buffer0 - tv) / xfade;
                    tmp = v0 + xfade;
                    if (tmp >= bufloc0 + bufsize) tmp -= bufsize;
                    tmp2 = (tmp >= bufloc0 + bufsize - 1) ? bufloc0 : tmp + 1;
                    ren0 = ren0 * frac + (1 - frac) * (buffer[(int)tmp + 0] * (1 - frac0) + buffer[(int)tmp2 + 0] * frac0);
                    ren1 = ren1 * frac + (1 - frac) * (buffer[(int)tmp + bufdiff] * (1 - frac0) + buffer[(int)tmp2 + bufdiff] * frac0);
                    if (tv + vr > buffer0 + 1) v0 += xfade;
                }
            }
            else
            {// read pointer moving slower than write pointer
                tv = v0;
                if (tv < buffer0) tv += bufsize;
                if (tv >= buffer0 && tv < buffer0 + xfade)
                {
                    // xfade
                    frac = (tv - buffer0) / xfade;
                    tmp = v0 + xfade;
                    if (tmp >= bufloc0 + bufsize) tmp -= bufsize;
                    tmp2 = (tmp >= bufloc0 + bufsize - 1) ? bufloc0 : tmp + 1;
                    ren0 = ren0 * frac + (1 - frac) * (buffer[(int)tmp + 0] * (1 - frac0) + buffer[(int)tmp2 + 0] * frac0);
                    ren1 = ren1 * frac + (1 - frac) * (buffer[(int)tmp + bufdiff] * (1 - frac0) + buffer[(int)tmp2 + bufdiff] * frac0);
                    if (tv + vr < buffer0 + 1) v0 += xfade;
                }
            }


            if ((v0 += vr) >= (bufloc0 + bufsize)) v0 -= bufsize;

            float os0 = spl0;
            float os1 = spl1;
            if (filter && pitch > 1.0)
            {

                t0 = spl0; t1 = spl1;
                spl0 = a1 * spl0 + a2 * h01 + a3 * h02 - b1 * h03 - b2 * h04 + denorm;
                spl1 = a1 * spl1 + a2 * h11 + a3 * h12 - b1 * h13 - b2 * h14 + denorm;
                h02 = h01; h01 = t0;
                h12 = h11; h11 = t1;
                h04 = h03; h03 = spl0;
                h14 = h13; h13 = spl1;
            }


            buffer[buffer0 + 0] = spl0; // write after reading it to avoid clicks
            buffer[buffer0 + bufdiff] = spl1;

            spl0 = ren0 * wetmix;
            spl1 = ren1 * wetmix;

            if (filter && pitch < 1.0)
            {
                t0 = spl0; t1 = spl1;
                spl0 = a1 * spl0 + a2 * h01 + a3 * h02 - b1 * h03 - b2 * h04 + denorm;
                spl1 = a1 * spl1 + a2 * h11 + a3 * h12 - b1 * h13 - b2 * h14 + denorm;
                h02 = h01; h01 = t0;
                h12 = h11; h11 = t1;
                h04 = h03; h03 = spl0;
                h14 = h13; h13 = spl1;
            }

            spl0 += os0 * drymix;
            spl1 += os1 * drymix;

            if ((buffer0 += 1) >= (bufloc0 + bufsize)) buffer0 -= bufsize;

        }
       
    }
}
