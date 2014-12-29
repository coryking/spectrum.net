using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{
    public class Adsr
    {

        public Adsr()
        {
            AttackSeconds = 0.5f;
            ReleaseSeconds = 1f;
            SustainSeconds = 1f;
            DecaySeconds = 1f;
        }
        public float AttackSeconds { get; set; }
        public float ReleaseSeconds { get; set; }
        public float SustainSeconds { get; set; }
        public float DecaySeconds { get; set; }

    }
}
