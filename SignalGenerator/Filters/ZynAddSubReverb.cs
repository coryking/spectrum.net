using CorySignalGenerator.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Filters
{
    public enum ReverbType
    {
        Random=0,
        Freeverb=1,
        Bandwidth=2
    }


    public class ReverbPreset
    {
        public static readonly Dictionary<String, float[]> Presets =
            new Dictionary<string, float[]>(){
            {"Cathedral1", new float[]{80,  64, 63,  24, 0,  0, 0, 85,  5,  83,  1, 64,  20}},
            {"Cathedral2", new float[]{80,  64, 69,  35, 0,  0, 0, 127, 0,  71,  0, 64,  20}},
            {"Cathedral3", new float[]{80,  64, 69,  24, 0,  0, 0, 127, 75, 78,  1, 85,  20}},
            {"Hall1", new float[]{90,  64, 51,  10, 0,  0, 0, 127, 21, 78,  1, 64,  20}},
            {"Hall2", new float[]{90,  64, 53,  20, 0,  0, 0, 127, 75, 71,  1, 64,  20}},
            {"Room1", new float[]{100, 64, 33,  0,  0,  0, 0, 127, 0,  106, 0, 30,  20}},
            {"Room2", new float[]{100, 64, 21,  26, 0,  0, 0, 62,  0,  77,  1, 45,  20}},
            {"Basement", new float[]{110, 64, 14,  0,  0,  0, 0, 127, 5,  71,  0, 25,  20}},
            {"Tunnel", new float[]{85,  80, 84,  20, 42, 0, 0, 51,  0,  78,  1, 105, 20}},
            {"Echoed1", new float[]{95,  64, 26,  60, 71, 0, 0, 114, 0,  64,  1, 64,  20}},
            {"Echoed2", new float[]{90,  64, 40,  88, 71, 0, 0, 114, 0,  88,  1, 64,  20}},
            {"VeryLong1", new float[]{90,  64, 93,  15, 0,  0, 0, 114, 0,  77,  0, 95,  20}},
            {"VeryLong2", new float[]{90,  64, 111, 30, 0,  0, 0, 114, 90, 74,  1, 80,  20}}
        };

        public ReverbPreset(string name, float volume, float panning, float time, float idelay, float idelayfb, float rdelay, float balance, float lpf, float hpf, float lohidamp, ReverbType type, float roomsize, float bandwidth)
        {
            // both rdelay and balance are not used... we keep them to make it easier to port the code

            Name = name;
            Volume = volume;
            Pan = panning;
            Time = time;
            InitialDelay = idelay;
            InitialDelayFeedback = idelayfb;
            LPF = lpf;
            HPF = hpf;
            LoHiDamp = lohidamp;
            Type = type;
            RoomSize = roomsize;
            Bandwidth = bandwidth;

        }

        /// <summary>
        ///  Create a new struct from a list of params.  This one is to make it easy to go from the c code
        /// </summary>
        /// <param name="paramList"></param>
        public ReverbPreset(string name, float[] paramList)
        {
            Name = name;
            Volume = paramList[0];
            Pan = paramList[1];
            Time = paramList[2];
            InitialDelay = paramList[3];
            InitialDelayFeedback = paramList[4];
            LPF = paramList[7];
            HPF = paramList[8];
            LoHiDamp = paramList[9];
            Type = (ReverbType)paramList[10];
            RoomSize = paramList[11];
            Bandwidth = paramList[12];

        }

        public string Name { get; set; }
        public float RoomSize;
        public float LoHiDamp;
        public float HPF;
        public float LPF;
        public float InitialDelayFeedback;
        public float InitialDelay;
        public float Pan;
        public float Volume;
        public float Time;
        public float Bandwidth;
        public ReverbType Type;
    }

    public class ZynAddSubReverb : Effect
    {
        object _lock = new object();

        public const int REV_COMBS=8;
        public const int REV_APS = 4;
        public const float Q = 0.5f;
        public const int BUFFER_SIZE = 400000; // pull this out of my ass...

        protected static Dictionary<ReverbType,int[]> combtunings = new Dictionary<ReverbType,int[]>(){
            //this is unused (for random)
            {ReverbType.Random, new int[]{0,    0,    0,    0,    0,    0,    0,    0      }},
            //Freeverb by Jezar at Dreampoint
            {ReverbType.Freeverb,new int[]{1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617   }},
            //duplicate of Freeverb by Jezar at Dreampoint
            {ReverbType.Bandwidth, new int[]{1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617   }}
        };

        protected static Dictionary<ReverbType,int[]> aptunings = new Dictionary<ReverbType,int[]>(){
            //this is unused (for random)
            {ReverbType.Random,new int[]{0,   0,   0,   0    }},
            //Freeverb by Jezar at Dreampoint
            {ReverbType.Freeverb,new int[]{225, 341, 441, 556  }},
            //duplicate of Freeverb by Jezar at Dreampoint
            {ReverbType.Bandwidth,new int[]{225, 341, 441, 556  }}
        };

        #region internal variables

        float outvolume; /**<This is the volume of effect and was public because
                          * it is needed in system effects.
                          * The out volume of such effects are always 1.0f, so
                          * this setting tells me how is the volume to the
                          * Master Output only.*/

        float volume;

        //Parameters
        int   lohidamptype;   //0=disable, 1=highdamp (lowpass), 2=lowdamp (highpass)
        int   idelaylen;
        int   idelayk;
        float lohifb;
        float idelayfb;
        float roomsize;
        float rs;   //rs is used to "normalise" the volume according to the roomsize
        int[]   comblen= new int[REV_COMBS * 2];
        int[]   aplen = new int[REV_APS * 2];
        Unison bandwidth;
        //class Unison * bandwidth;

        //Internal Variables
        float[][] comb = new float[REV_COMBS * 2][]; // PORT: this was a pointer (float *comb[REV_COMBS*2])
        int[]    combk = new int[REV_COMBS * 2];
        float[]  combfb = new float[REV_COMBS * 2]; //feedback-ul fiecarui filtru "comb"
        float[]  lpcomb = new float[REV_COMBS * 2]; //pentru Filtrul LowPass
        float[][] ap = new float[REV_APS * 2][]; // PORT: this was a pointer (float *ap[REV_APS])
        int[]    apk = new int[REV_APS * 2];
        float[] idelay;
        NAudio.Dsp.BiQuadFilter lpfFilter;
        NAudio.Dsp.BiQuadFilter hpfFilter;
        //class AnalogFilter * lpf, *hpf; //filters

        // pan junk
        float pangainL;
        float pangainR;
        char Plrcross; // L/R mix
        float lrcross;

        Random rnd = new Random();

        #endregion

        public ZynAddSubReverb(WaveFormat format) :base(format)
        {

        }

        public ZynAddSubReverb(ISampleProvider source) : base(source)
        {

        }
        protected override void Init()
        {
            // set our defaults
            idelaylen = 0;
            roomsize = 1f;
            rs = 1f;
            
            Volume = 48;
            Time = 64;
            InitialDelay = 40f;
            InitialDelayFeedback = 0;
            LPF = 127;
            HPF = 0;
            LoHiDamp = 80;
            Type = ReverbType.Freeverb;
            RoomSize = 64;
            Bandwidth = 30;
            

            foreach (var item in ReverbPreset.Presets)
            {
                _presets.Add(new ReverbPreset(item.Key, item.Value));
            }

            SetPresetCommand = new Models.RelayCommand(OnSetPresetCommand);

            for (int i = 0; i < REV_COMBS * 2; i++)
            {
                comblen[i] = 800 + (int)(rnd.NextDouble() * 1400f);
                combk[i] = 0;
                lpcomb[i] = 0;
                combfb[i] = -.97f;
                comb[i] = null;
            }
            for (int i = 0; i < REV_APS * 2; i++)
            {
                aplen[i] = 500 + (int)(rnd.NextDouble() * 500f);
                apk[i] = 0;
                ap[i] = null;

            }
            LoadFromPreset(Presets[0]); // our default preset...
            Reset(); // do not call this before the comb initialisation;
        }

        public Models.RelayCommand SetPresetCommand { get; set; }
        protected void OnSetPresetCommand(object parameter)
        {
            var preset = parameter as ReverbPreset;
            if(preset == null)
                return;

            LoadFromPreset(preset);
        }

        private List<ReverbPreset> _presets = new List<ReverbPreset>();
        public IReadOnlyList<ReverbPreset> Presets
        {
            get
            {
                return _presets;
            }
        }

        public void LoadFromPreset(ReverbPreset preset)
        {
            Volume = preset.Volume;
            Pan = preset.Pan;
            Time = preset.Time;
            InitialDelay = preset.InitialDelay;
            InitialDelayFeedback = preset.InitialDelayFeedback;
            LPF = preset.LPF;
            HPF = preset.HPF;
            LoHiDamp = preset.LoHiDamp;
            Type = preset.Type;
            RoomSize = preset.RoomSize;
            Bandwidth = preset.Bandwidth;
            lock (_lock)
            {
                settype();
            }
        }

        #region internal param setters

        private Action LockedSet(Action callback)
        {
            return () =>
            {
                lock (_lock)
                {
                    callback.Invoke();
                }
            };
        }
        protected void setpan()
        {
            float t = (Pan > 0) ? (float)(Pan - 1) / 126f : 0f;
            pangainL = cos(t * PI / 2f);
            pangainR = cos((1f - t) * PI / 2f);

        }

        protected void setvolume()
        {
            // there was code for when this was an insertion effect
            volume = outvolume = Volume / 127f;
            if (Volume == 0)
                Reset();
        }

        protected void settime()
        {
            float t = pow(60f, Time / 127f) - 0.97f;
            for (int i = 0; i < REV_COMBS * 2; i++)
            {
                combfb[i] = -exp(comblen[i] / SampleRate * log(0.001f) / t);
            }
            //the feedback is negative because it removes the DC
        }

        protected void setLoHiDamp()
        {
            if(LoHiDamp == 64)
            {
                lohidamptype = 0;
                lohifb = 0f;
            }
            else
            {
                if (LoHiDamp < 64)
                    lohidamptype = 1;
                if (LoHiDamp > 64)
                    lohidamptype = 2;
                float x = abs((LoHiDamp - 60f) / 64.1f);
                lohifb = x * x;
            }
        }

        protected void setiDelay()
        {
            float delay = pow(50f * InitialDelay / 127f, 2f) - 1f;
            int newDelayLen = (int)(SampleRate * delay / 1000);
            if (newDelayLen == idelaylen)
                return;

            idelay = null;

            idelaylen = newDelayLen;
            if (idelaylen > 1)
            {
                idelay = new float[idelaylen];
                idelayk = 0;
            }

        }

        protected void setiDelayFb()
        {
            idelayfb = InitialDelayFeedback / 128f;
        }

        protected void sethpf()
        {
            if (HPF == 0)
                hpfFilter = null;
            else
            {
                float fr = exp(sqrt(HPF / 127f) * log(10000f)) + 20f;
                hpfFilter = NAudio.Dsp.BiQuadFilter.HighPassFilter(SampleRate, fr, Q);
            }
        }

        protected void setlpf()
        {
            if (LPF == 0)
                lpfFilter = null;
            else
            {
                float fr = exp(sqrt(HPF / 127f) * log(25000f)) + 40.0f;
                hpfFilter = NAudio.Dsp.BiQuadFilter.LowPassFilter(SampleRate, fr, Q);
            }
        }

        protected void settype()
        {
            float samplerate_adjust = SampleRate / 44100.0f;
            float tmp = 0;
            for (int i = 0; i < REV_COMBS * 2; ++i)
            {
                if (Type == ReverbType.Random)
                    tmp = 800.0f + (int)(rnd.NextDouble() * 1400.0f);
                else
                    tmp = combtunings[Type][i % REV_COMBS];
                tmp *= roomsize;
                if (i > REV_COMBS)
                    tmp += 23.0f;
                tmp *= samplerate_adjust; //adjust the combs according to the samplerate
                if (tmp < 10.0f)
                    tmp = 10.0f;
                combk[i] = 0;
                lpcomb[i] = 0;
                if (comblen[i] != (int)tmp || comb[i] == null)
                {
                    comblen[i] = (int)tmp;
                    comb[i] = new float[comblen[i]];
                    
                }
            }

            for (int i = 0; i < REV_APS * 2; ++i)
            {
                if (Type == ReverbType.Random)
                    tmp = 500 + (int)(rnd.NextDouble() * 500.0f);
                else
                    tmp = aptunings[Type][i % REV_APS];
                tmp *= roomsize;
                if (i > REV_APS)
                    tmp += 23.0f;
                tmp *= samplerate_adjust; //adjust the combs according to the samplerate
                if (tmp < 10)
                    tmp = 10;
                apk[i] = 0;
                if (aplen[i] != (int)tmp || ap[i] == null)
                {
                    aplen[i] = (int)tmp;
                    ap[i] = new float[aplen[i]];
                }
            }
            bandwidth = null;
            if(Type==ReverbType.Bandwidth)
            { //bandwidth
                //TODO the size of the unison buffer may be too small, though this has
                //not been verified yet.
                //As this cannot be resized in a RT context, a good upper bound should
                //be found
                bandwidth = new Unison(BUFFER_SIZE / 4 + 1, 2.0f, SampleRate);
                bandwidth.Size = 50;
                bandwidth.BaseFrequency = 1.0f;
            }
            settime();
            Reset();
        }

        protected void setroomsize()
        {
            roomsize = (RoomSize - 64f) / 64f;
            if (roomsize > 0f)
                roomsize *= 2;
            roomsize = pow(10f, roomsize);
            rs = sqrt(roomsize);
            settype();
        }


        protected void setbandwidth()
        {
            float v = Bandwidth / 127f;
            if (bandwidth != null)
                bandwidth.Bandwidth = pow(v, 2f) * 200f;
        }        
        #endregion
        protected override void Reset()
        {
            for (int i = 0; i < REV_COMBS * 2; i++)
            {
                lpcomb[i] = 0f;
                Array.Clear(comb[i], 0, comblen[i]);
            }
            for (int i = 0; i < REV_APS * 2; i++)
            {
                Array.Clear(ap[i], 0, aplen[i]);
            }
            if (idelay != null)
                Array.Clear(idelay, 0, idelaylen);
            
        }



        #region Properties

        #region Property Type
        private ReverbType _type;

        /// <summary>
        /// Sets and gets the Type property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ReverbType Type
        {
            get
            {
                return _type;
            }
            set
            {
                Set(ref _type, value, changeCallback: LockedSet(() => { settype(); }));
            }
        }
        #endregion

        
        #region Property Bandwidth
        private float _bandwidth;

        /// <summary>
        /// Sets and gets the Bandwidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Bandwidth
        {
            get
            {
                return _bandwidth;
            }
            set
            {
                Set(ref _bandwidth, value, changeCallback: LockedSet(() => { setbandwidth(); }));
            }
        }
        #endregion
		


        #region Property RoomSize
        private float _roomSize;

        /// <summary>
        /// Sets and gets the RoomSize property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float RoomSize
        {
            get
            {
                return _roomSize;
            }
            set
            {
                Set(ref _roomSize, value, changeCallback: LockedSet(() => { setroomsize(); }));
            }
        }
        #endregion
		
        
        #region Property LoHiDamp
        private float _loHiDamp;

        /// <summary>
        /// Sets and gets the LoHiDamp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float LoHiDamp
        {
            get
            {
                return _loHiDamp;
            }
            set
            {
                Set(ref _loHiDamp, value, changeCallback: LockedSet(()=>setLoHiDamp()));
            }
        }
        #endregion
		

        #region Property HPF
        private float _hpf;

        /// <summary>
        /// Sets and gets the HPF property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float HPF
        {
            get
            {
                return _hpf;
            }
            set
            {
                Set(ref _hpf, value, changeCallback: LockedSet(()=>sethpf()));
            }
        }
        #endregion
		

        #region Property LPF
        private float _lpf;

        /// <summary>
        /// Sets and gets the LPF property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float LPF
        {
            get
            {
                return _lpf;
            }
            set
            {
                Set(ref _lpf, value, changeCallback: LockedSet(()=>setlpf()));
            }
        }
        #endregion
		
        #region Property InitialDelayFeedback
        private float _initialDelayFeedback = 0f;

        /// <summary>
        /// Sets and gets the InitialDelayFeedback property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float InitialDelayFeedback
        {
            get
            {
                return _initialDelayFeedback;
            }
            set
            {
                Set(ref _initialDelayFeedback, value, changeCallback:LockedSet(()=>setiDelayFb()));
            }
        }
        #endregion
		

        #region Property InitialDelay
        private float _initialDelay;

        /// <summary>
        /// Sets and gets the InitialDelay property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float InitialDelay
        {
            get
            {
                return _initialDelay;
            }
            set
            {
                Set(ref _initialDelay, value, changeCallback:LockedSet(()=>setiDelay()));
            }
        }
        #endregion
		
        #region Property Pan
        private float _pan;

        /// <summary>
        /// Sets and gets the Pan property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float  Pan
        {
            get
            {
                return _pan;
            }
            set
            {
                Set(ref _pan, value, changeCallback:LockedSet(()=>setpan()));
            }
        }
        #endregion
		

        #region Property Volume
        private float _volume;

        /// <summary>
        /// Sets and gets the Volume property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                Set(ref _volume, value, LockedSet(()=>setvolume()));
            }
        }
        #endregion
		

        #region Property Time
        private float _time;

        /// <summary>
        /// Sets and gets the Time property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Time
        {
            get
            {
                return _time;
            }
            set
            {
                Set(ref _time, value, LockedSet(()=>settime()));
            }
        }

        #endregion

        #endregion //Properties

        protected override int OnRead(float[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                var samplesRead = Source.Read(buffer, offset, count);
                if (Volume == 0)
                    return samplesRead;

                int bufferSize = count / 2;
                float[] inputbuf = new float[bufferSize];
                int inBuffOffset = offset;
                for (int i = 0; i < bufferSize; i++)
                    inputbuf[i] = (buffer[inBuffOffset++] + buffer[inBuffOffset++]) / 2f;
                if(idelay != null)
                {
                    for (int i = 0; i < bufferSize; i++)
                    {
                        float tmp = inputbuf[i] + idelay[idelayk] * idelayfb;
                        inputbuf[i] = idelay[idelayk];
                        idelay[idelayk] = tmp;
                        idelayk++;
                        if (idelayk >= idelaylen)
                            idelayk = 0;
                    }
                }
                if (bandwidth != null)
                    bandwidth.Process(bufferSize, inputbuf);

                for (int i = 0; i < bufferSize; i++)
                {
                    if (lpfFilter != null)
                        inputbuf[i] = lpfFilter.Transform(inputbuf[i]);
                    if (hpfFilter != null)
                        inputbuf[i] = hpfFilter.Transform(inputbuf[i]);
                }
                
                float[] efxoutl = new float[bufferSize];
                float[] efxoutr = new float[bufferSize];
                processmono(0, efxoutl, inputbuf, bufferSize);
                processmono(0, efxoutr, inputbuf, bufferSize);

                float lvol = rs / REV_COMBS * pangainL;
                float rvol = rs / REV_COMBS * pangainR;
                //if(insertion == true)
                //{
                //    lvol *= 2f;
                //    rvol *= 2f;
                //}
                VectorMath.vadd(buffer, offset, 2, 1f, efxoutl, 0, 1, lvol, buffer,offset, 2, bufferSize);
                VectorMath.vadd(buffer, offset + 1, 2, 1f, efxoutr, 0, 1, rvol, buffer,offset  + 2, 2, bufferSize);

                return bufferSize * 2;
            }
        }

        private unsafe void processmono(int ch, float[] output, float[] inputbuf, int bufferSize)
        {
            for (int j = REV_COMBS * ch; j < REV_COMBS * (ch + 1); j++)
            {
                // this is kind of awkward, but it is as close to the original c++ code as I could make it
                fixed (int* ck = &combk[j])
                {
                    fixed (float* lpcombj = &lpcomb[j])
                    {
                        int comblength = comblen[j];

                        for (int i = 0; i < bufferSize; i++)
                        {
                            float fbout = comb[j][*ck] * combfb[j];
                            fbout = fbout * (1f - lohifb) + *lpcombj * lohifb;
                            *lpcombj = fbout;


                            comb[j][*ck] = inputbuf[i] + fbout;
                            output[i] += fbout;

                            *ck += 1;
                            if (*ck >= comblength)
                                *ck = 0;
                        }

                    }
                }
            }
            for (int j = REV_APS*ch; j <  REV_APS * (1 + ch); j++)
            {
                fixed (int* ak = &apk[j])
                {
                    int aplength = aplen[j];
                    for (int i = 0; i < bufferSize; i++)
                    {
                        float tmp = ap[j][*ak];
                        ap[j][*ak] = 0.7f * tmp + output[i];
                        output[i] = tmp - 0.7f * ap[j][*ak];
                        *ak += 1;
                        if (*ak >= aplength)
                            *ak = 0;
                    }
                }
            }

        }

        public override string Name
        {
            get { return "ZynAddSub Reverb"; }
        }

        /// <summary>
        /// This requires stereo.
        /// </summary>
        protected override bool RequiresStereo
        {
            get
            {
                return true;
            }
        }
    }
}
