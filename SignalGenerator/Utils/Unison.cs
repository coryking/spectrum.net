using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CorySignalGenerator.Utils
{
    public class Unison
    {
        private int p1;
        private float p2;
        private int SampleRate;

        public Unison(int p1, float p2, int SampleRate)
        {
            // TODO: Complete member initialization
            this.p1 = p1;
            this.p2 = p2;
            this.SampleRate = SampleRate;
        }
        public float BaseFrequency { get; set; }

        public int Size { get; set; }

        public float Bandwidth { get; set; }

        internal void Process(int bufferSize, float[] inputbuf)
        {
            throw new NotImplementedException();
        }
    }
}
