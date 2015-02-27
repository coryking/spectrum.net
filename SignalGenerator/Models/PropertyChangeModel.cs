using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{
    public delegate void PropertyChangeModelCallback<T>(T oldValue, T newValue);

    public class PropertyChangeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T newValue = default(T), PropertyChangeModelCallback<T> changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            var oldValue = field;
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;

            OnPropertyChanged(propertyName);
            if (changeCallback != null)
                changeCallback.Invoke(oldValue,newValue);

            return true;
        }

        protected bool Set(ref float field, float newValue, float minValue, PropertyChangeModelCallback<float> changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (float)Math.Max(minValue, newValue), changeCallback, propertyName);
        }

        protected bool Set(ref float field, float newValue, float minValue, float maxValue, PropertyChangeModelCallback<float> changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (float)Math.Min(maxValue, newValue), minValue, changeCallback, propertyName);
        }
        protected bool Set(ref int field, int newValue, int minValue, PropertyChangeModelCallback<int> changeCallback = null, [CallerMemberName] string propertyName = null)
        {
            return Set(ref field, (int)Math.Max(minValue, newValue), changeCallback, propertyName);
        }

        protected bool Set(ref int field, int newValue, int minValue, int maxValue, PropertyChangeModelCallback<int> changeCallback = null, [CallerMemberName] string propertyName = null)
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
