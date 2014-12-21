﻿using System;
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
        public NAudio.Wave.SampleProviders.SignalGeneratorType Type { get; set; }

        private float _lowPassCutoff;

        public float LowPassCutoff
        {
            get { return _lowPassCutoff; }
            set { _lowPassCutoff = value; OnPropertyChanged("LowPassCutoff"); }
        }

        private float _lowPassResonance;

        public float LowPassResonance
        {
            get { return _lowPassResonance; }
            set { _lowPassResonance = value; OnPropertyChanged("LowPassresonance"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}
