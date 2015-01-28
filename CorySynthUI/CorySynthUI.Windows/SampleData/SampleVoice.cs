using CorySignalGenerator.Sequencer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySynthUI.SampleData
{

    public class SampleVoices : List<SampleDataVoice>
    {
        public SampleVoices() : base()
        {
            this.Add(new SampleDataVoice("Super Voice"));
            this.Add(new SampleDataVoice("Mega Voice"));
        }
    }

    public class SampleDataVoice : IVoice

    {
        public SampleDataVoice()
        {

        }

        public SampleDataVoice(string name) :this()
        {
            Name = name;
        }

        public string Name
        {
            get;
            set;
        }

        public float Volume
        {
            get;
            set;
        }

        public void NoteOn(CorySignalGenerator.Models.MidiNote note, float velocity)
        {
            throw new NotImplementedException();
        }

        public void NoteOff(CorySignalGenerator.Models.MidiNote note)
        {
            throw new NotImplementedException();
        }

        public void SustainOn()
        {
            throw new NotImplementedException();
        }

        public void SustainOff()
        {
            throw new NotImplementedException();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get;
            set;
        }
    }
}
