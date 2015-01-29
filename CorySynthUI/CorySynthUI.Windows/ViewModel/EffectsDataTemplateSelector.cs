using CorySignalGenerator.Filters;
using CorySignalGenerator.Sequencer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorySynthUI.ViewModel
{
    public class EffectsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GhettoReverbDataTemplate { get; set; }

        public DataTemplate ChorusDataTemplate { get; set; }

        public DataTemplate BandpassFilterDataTemplate { get; set; }

        public DataTemplate FlangerEffectDataTemplate { get; set; }


        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            // All we know how to handle is the SamplerVoice right now...
            var effect = item as IEffect;

            if (effect == null)
                return base.SelectTemplateCore(item, container);

            var effectType = effect.GetType();
            if (effectType == typeof(GhettoReverb))
                return GhettoReverbDataTemplate;

            if (effectType == typeof(ChorusEffect))
                return ChorusDataTemplate;

            if (effectType == typeof(FourPolesLowPassFilter))
                return BandpassFilterDataTemplate;

            if (effectType == typeof(FlangerEffect))
                return FlangerEffectDataTemplate;


            return base.SelectTemplateCore(item, container);
        }

    }
}
