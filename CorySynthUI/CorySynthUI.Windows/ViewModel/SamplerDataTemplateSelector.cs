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
            var voice = item as SamplerVoice;

            if (voice == null)
                return base.SelectTemplateCore(item, container);

            var samplerType = voice.Sampler;
            if (samplerType is PadSound)
                return PadSoundDataTemplate;

            if (samplerType is SignalGeneretedSound)
                return SynthSoundDataTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
