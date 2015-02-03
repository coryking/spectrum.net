﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{
    public class PropertyChangeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T newValue = default(T), Action changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;

            OnPropertyChanged(propertyName);
            if (changeCallback != null)
                changeCallback.Invoke();

            return true;
        }

        protected bool Set(ref float field, float newValue, float minValue, Action changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (float)Math.Max(minValue, newValue), changeCallback, propertyName);
        }

        protected bool Set(ref float field, float newValue, float minValue, float maxValue, Action changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (float)Math.Min(maxValue, newValue), minValue, changeCallback, propertyName);
        }
        protected bool Set(ref int field, int newValue, int minValue, Action changeCallback = null,[CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (int)Math.Max(minValue, newValue), changeCallback, propertyName);
        }

        protected bool Set(ref int field, int newValue, int minValue, int maxValue, Action changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (int)Math.Min(maxValue, newValue), minValue, changeCallback, propertyName);
        }



        protected virtual void HandlePropertyChanged(string propertyName){
            // do nothings
        }

        protected void OnPropertyChanged(string propertyName)
        {
            HandlePropertyChanged(propertyName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
