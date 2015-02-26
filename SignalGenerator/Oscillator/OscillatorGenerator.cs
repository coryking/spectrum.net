using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Oscillator
{
    public class OscillatorGenerator : Models.PropertyChangeModel
    {
        /// <summary>
        ///  Max number of user-adjustable harmonics
        /// </summary>
        public const int MAX_HARMONICS = 128;
        /// <summary>
        ///  Size of the oscillator sample
        /// </summary>
        public const int OSCILLATOR_SIZE = 1024;

        private Random rnd;
        private object _lock = new object();

        public OscillatorGenerator()
        {
            rnd = new Random();
            InitHarmonics();
            Prepare();
        }

        public float[] GetFrequencies(float freqHz)
        {

            PrepareIfNeeded();

            var input = FFTFrequencies;
            var output = new Complex[OSCILLATOR_SIZE];

            int nyquist = OSCILLATOR_SIZE / 2;

            Array.Copy(input, output, nyquist - 1);

            // TODO: Any adaptive harmonics & post processing...

            // TODO: Apply any resonance...

            FrequencyUtils.RmsNormalize(output, OSCILLATOR_SIZE/2);
            
            var outputFloat = new float[OSCILLATOR_SIZE];
            for (int i = 1; i < OSCILLATOR_SIZE /2; i++)
                outputFloat[i - 1] = (float)output[i].Magnitude;
            
            return outputFloat;
        }

        private void PrepareIfNeeded()
        {
            lock (_lock)
            {
                if (NeedsPrepare)
                    Prepare();
            }
        }

        

        protected void Prepare()
        {
            Debug.WriteLine("Maybe Preparing OscillatorGenerator");
            if (dirtyBaseFunction || BaseFunctionFFTFrequencies == null)
            {
                ChangeBaseFunction();
                dirtyBaseFunction = false;
            }
            var magnitudes = new double[MAX_HARMONICS];
            var phases = new double[MAX_HARMONICS];
            //var harmonics = new Complex[MAX_HARMONICS];
            for (int i = 0; i < MAX_HARMONICS; i++)
            {
                magnitudes[i] = Harmonics[i].GetMagnitude();
                phases[i] = Harmonics[i].GetPhase();
                //harmonics[i] = Harmonics[i].ToComplex();
            }


            PrepareFFTFrequencies();
            if(BaseFunction == null || BaseFunction is SineBaseFunction)
            {
                Debug.WriteLine("Making FFT Frequencies for SineBaseFunction");
                for (int i = 0; i < MAX_HARMONICS-1; i++)
                {
                    // I'm kind of positive there is a function built into Complex that does this...
                    var real = -magnitudes[i] * Math.Sin(phases[i] * (i + 1.0)) / 2.0;
                    var imaginary = magnitudes[i] * Math.Cos(phases[i] * (i + 1.0)) / 2.0;

                    FFTFrequencies[i + 1] = new Complex(real, imaginary);
                }
            }
            else
            {
                Debug.WriteLine("Making FFT Frequencies for all but SineBaseFunction");
                for (int j = 0; j < MAX_HARMONICS; j++)
                {
                    if (Harmonics[j].Magnitude == 64)
                        continue;
                    for (int i = 1; i < OSCILLATOR_SIZE/2; i++)
                    {
                        var k = i * (j + 1);
                        if (k >= OSCILLATOR_SIZE / 2)
                            break;

                        var rotated = FrequencyUtils.FromPolar(magnitudes[j], phases[j] * k);
                        FFTFrequencies[k] += BaseFunctionFFTFrequencies[i] * rotated;
                    }
                    
                }
            }

            // TODO: Any waveshaping and/or oscillator filtering

            // TODO: Any modulation or spectrum adjustments
            
            // Clear DC
            FFTFrequencies[0] = new Complex();

            var sp = new StringBuilder();
            var str = String.Join("\t", FFTFrequencies.Take(OSCILLATOR_SIZE/2).Select(x=>x.Magnitude));
            sp.AppendFormat("FFT Frequencies,{0}\n", str);

            var bstr = String.Join("\t", BaseFunctionFFTFrequencies.Take(OSCILLATOR_SIZE/2).Select(x=>x.Magnitude));
            sp.AppendFormat("Base Frequencies,{0}\n", bstr);
            
            var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.SetData(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text, sp.ToString());
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
            });

            oscPrepared = true;
            dirtyParams = false;
            Debug.WriteLine("Done with Prepare");
        }

       
        private void PrepareFFTFrequencies()
        {
            Debug.WriteLine("Preparing FFT Fequencies");
            if (FFTFrequencies == null)
                FFTFrequencies = new Complex[OSCILLATOR_SIZE];
            else
                Array.Clear(FFTFrequencies, 0, OSCILLATOR_SIZE);
        }
        /// <summary>
        /// Change the base function, which means re-compute all the fft stuff
        /// </summary>
        protected void ChangeBaseFunction()
        {
            Debug.WriteLine("Changing Base Function");
            if(BaseFunction != null)
            {
                var samples = GetBaseFunctionSamples(BaseFunction, BaseFunctionParameter);
                BaseFunctionFFTFrequencies = samples.Select((x) => { return new Complex(x, 0.0); }).ToArray();
                MathNet.Numerics.IntegralTransforms.Fourier.Radix2Forward(
                    BaseFunctionFFTFrequencies,
                    MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
                // clear DC
                BaseFunctionFFTFrequencies[0] = new Complex();
            }
            else
            {
                BaseFunctionFFTFrequencies = new Complex[OSCILLATOR_SIZE];
            }
        }

        /// <summary>
        /// Get the samples from the base function
        /// </summary>
        /// <param name="baseFunction">The function to call</param>
        /// <param name="param">The parameter for the function</param>
        /// <remarks>We pass in the function & params because the property may change on us while this function is at work</remarks>
        private float[] GetBaseFunctionSamples(IBaseFunction baseFunction, byte param)
        {
            var result = new float[OSCILLATOR_SIZE];

            var parameter = (param + 0.5f) / 128.0f;
            // deal with rounding...
            if (param == 64)
                parameter = 0.5f;

            // TODO: Function Modulation

            for (int i = 0; i < OSCILLATOR_SIZE; i++)
            {
                float t = i * 1.0f / OSCILLATOR_SIZE;

                // TODO: Base Function Modulation
                t = t - FloatMath.floor(t);

                if (BaseFunction != null && !(baseFunction is SineBaseFunction))
                    result[i] = baseFunction.Process(t, parameter);
                else
                    result[i] = -FloatMath.sin(2.0f * FloatMath.PI * i / OSCILLATOR_SIZE);

            }

            return result;
        }

        #region Properties

        public Complex[] FFTFrequencies { get; private set; }

        protected Complex[] BaseFunctionFFTFrequencies { get; set; }

        /// <summary>
        /// If true, this oscillator needs to be prepared
        /// </summary>
        protected bool NeedsPrepare
        {
            get
            {
                return dirtyBaseFunction || dirtyParams || !oscPrepared;
            }
        }

        #region Harmonics Property


        /// <summary>
        /// User settable harmonics
        /// </summary>
        public ObservableCollection<Harmonic> Harmonics { get; private set; }

        private void InitHarmonics()
        {
            Debug.WriteLine("Building out Harmonics");
            Harmonics = new ObservableCollection<Harmonic>();

            for (int i = 0; i < MAX_HARMONICS; i++)
            {
                Harmonic harmonic = null;
                if (i == 0)
                    harmonic = new Harmonic(i,127, 64);
                else
                    harmonic = new Harmonic(i);

                harmonic.PropertyChanged += HarmonicsPropertyChanged;
                Harmonics.Add(harmonic);
            }

            //Debug.WriteLine("About to do prop changed");
            //OnPropertyChanged("Harmonics");
            Debug.WriteLine("Done with prop changed");
        }

        private void HarmonicsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Some property in harmonics changed: {0}.  {1}", e.PropertyName, sender);
            dirtyParams = true;
        }

        #endregion

        private bool dirtyBaseFunction = false;
        private bool dirtyParams = false;
        private bool oscPrepared = false;


        #region Property Randomness
        private byte _randomness = 127;

        /// <summary>
        /// Amount of randomness...  best to leave this alone.
        /// </summary>
        public byte Randomness
        {
            get
            {
                return _randomness;
            }
            set
            {
                Set(ref _randomness, value);
            }
        }
        #endregion
		

        #region Property BaseFunction
        private IBaseFunction _baseFunction = null;

        /// <summary>
        /// Base Oscillator Function
        /// </summary>
        public IBaseFunction BaseFunction
        {
            get
            {
                return _baseFunction;
            }
            set
            {
                Set(ref _baseFunction, value, changeCallback: () => { dirtyBaseFunction = true; });
            }
        }
        #endregion

        #region Property BaseFunctionParameter
        private byte _baseFunctionParameter = 0;

        /// <summary>
        /// Gets / Sets the parameter for the base function
        /// </summary>
        public byte BaseFunctionParameter
        {
            get
            {
                return _baseFunctionParameter;
            }
            set
            {
                Set(ref _baseFunctionParameter, value);
            }
        }
        #endregion
		
        #endregion

        protected override void HandlePropertyChanged(string propertyName)
        {
            Debug.WriteLine("{0} property changed in OscillatorGenerator", propertyName);
            dirtyParams = true;
            base.HandlePropertyChanged(propertyName);
        }
    }
}
