using CorySignalGenerator.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySynthUI.ViewModel
{
    public class AvailableVoice
    {
        public AvailableVoice(string name, Type type)
        {
            Name = name;
            Type = type;

        }
        public String Name { get; set; }
        public Type Type { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class AvailableVoices
    {
        public static IEnumerable<AvailableVoice> GetChoices
        {
            get
            {
                return new List<AvailableVoice>()
                {
                    new AvailableVoice("Pad Sound",typeof(PadSound)),
                    new AvailableVoice("Synth Sound",typeof(SignalGeneretedSound)),
                };
            }
        }

    }
}
