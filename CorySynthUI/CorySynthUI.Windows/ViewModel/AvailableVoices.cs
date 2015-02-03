using CorySignalGenerator.Filters;
using CorySignalGenerator.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySynthUI.ViewModel
{
    public class AvailableType
    {
        public AvailableType(string name, Type type)
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
        public static IEnumerable<AvailableType> Choices
        {
            get
            {
                return new List<AvailableType>()
                {
                    new AvailableType("Pad Sound",typeof(PadSound)),
                    new AvailableType("Synth Sound",typeof(SignalGeneretedSound)),
                };
            }
        }

    }

    public class AvailableEffects
    {
        public static IEnumerable<AvailableType> Choices
        {
            get
            {
                return new List<AvailableType>()
                {
                    new AvailableType("Ghetto Reverb", typeof(GhettoReverb)),
                    new AvailableType("Chorus", typeof(ChorusEffect)),
                    new AvailableType("Bandpass Filter", typeof(FourPolesLowPassFilter)),
                    new AvailableType("Flanger", typeof(FlangerEffect)),
                    new AvailableType("ZynAddSub Reverb", typeof(ZynAddSubReverb)),
                };
            }
        }   
    }
}
