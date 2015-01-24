using CorySignalGenerator.Sequencer;
using CorySignalGenerator.Sequencer.Interfaces;
using CorySignalGenerator.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorySynthUI.ViewModel
{
    public class SamplerDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Reference to the PadSoundDataTemplate
        /// </summary>
        public DataTemplate PadSoundDataTemplate { get; set; }
        /// <summary>
        /// Reference to the SynthSoundDataTemplate
        /// </summary>
        public DataTemplate SynthSoundDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            // All we know how to handle is the SamplerVoice right now...
            var sampler = item as ISampler;

            if (sampler == null)
                return base.SelectTemplateCore(item, container);

            var samplerType = sampler.GetType();
            if (samplerType == typeof(PadSound))
                return PadSoundDataTemplate;

            if (samplerType == typeof(SignalGeneretedSound))
                return SynthSoundDataTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
