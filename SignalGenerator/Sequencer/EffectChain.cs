using CorySignalGenerator.Models;
using CorySignalGenerator.Sequencer.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Sequencer
{
    public class EffectChain : SignalChain<IEffect>
    {
        public EffectChain(ISampleProvider source) : base(source)
        {
            AddEffectCommand = new RelayCommand(OnAddEffectCommand);
            RemoveEffectCommand = new RelayCommand(OnRemoveEffectCommand);
        }

        #region Relay Commands
        public RelayCommand AddEffectCommand { get; private set; }

        private void OnAddEffectCommand(object parameter)
        {
            var effectType = parameter as Type;
            if (effectType == null)
                return;

            var effect = Activator.CreateInstance(effectType, WaveFormat) as IEffect;
            if (effect == null)
                return;

            this.Add(effect);
        }

        public RelayCommand RemoveEffectCommand { get; private set; }

        private void OnRemoveEffectCommand(object parameter)
        {
            var effect = parameter as IEffect;

            if (effect != null)
                this.Remove(effect);

        }

        #endregion

        #region Properties


        private IEffect _selectedEffect = null;
        /// <summary>
        /// The selected effect.
        /// </summary>
        /// <remarks>I don't like this here... but it is here to help out data templating</remarks>
        public IEffect SelectedEffect
        {
            get { return _selectedEffect; }
            set
            {
                if (value == _selectedEffect)
                    return;
                _selectedEffect = value;
                OnPropertyChanged("SelectedEffect");

            }
        }

        #endregion
    }
}
