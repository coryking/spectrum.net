using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CorySynth.Models
{
    public class SignalModel : INotifyPropertyChanged
    {
        public SignalModel()
        {
            LowPassCutoff = 22000.0f;
            Q = 0.5f;
            ReverbDelay = 100;
            ReverbDecay = 0.2f;

        }

        public NAudio.Wave.SampleProviders.SignalGeneratorType Type { get; set; }

        private float _reverbDelay;

        public float ReverbDelay
        {
            get { return _reverbDelay; }
            set
            {
                if (_reverbDelay != value)
                {
                    _reverbDelay = value;
                    OnPropertyChanged("ReverbDelay");
                }
            }
        }

        private float _reverbDecay;

        public float ReverbDecay
        {
            get { return _reverbDecay; }
            set {
                if (_reverbDecay != value)
                {
                    _reverbDecay = value;
                    OnPropertyChanged("ReverbDecay");
                }

            }
        }

        private float _lowPassCutoff;

        public float LowPassCutoff
        {
            get { return _lowPassCutoff; }
            set
            {
                if (_lowPassCutoff != value)
                {
                    _lowPassCutoff = value; 
                    OnPropertyChanged("LowPassCutoff");
                }

            }
        }

        private float _q;

        public float Q
        {
            get { return _q; }
            set
            {
                if (_q != value)
                {
                    _q = value;
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
    }
}
