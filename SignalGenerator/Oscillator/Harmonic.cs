using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Oscillator
{
    public enum MagnitudeType
    {
        Linear=0,
        /// <summary>
        /// dB Scale (-40)
        /// </summary>
        dB_40=1,
        /// <summary>
        /// dB Scale (-40)
        /// </summary>
        dB_60=2,
        /// <summary>
        /// dB Scale (-40)
        /// </summary>
        dB_80=3,
        /// <summary>
        /// dB Scale (-40)
        /// </summary>
        dB_100=4
    }

    /// <summary>
    /// Midi parameters for magnitude & phase.
    /// </summary>
    public class Harmonic : Models.PropertyChangeModel
    {
        public Harmonic() : this(64,64)
        {

        }
        public Harmonic(byte magnitude, byte phase)
        {
            this._magnitude = magnitude;
            this._phase = phase;
        }

        #region Property Magnitude
        private byte _magnitude = 0;

        /// <summary>
        /// Harmonic Magnitude (0 -> 127)
        /// </summary>
        public byte Magnitude
        {
            get
            {
                return _magnitude;
            }
            set
            {
                Set(ref _magnitude, value);
            }
        }
        #endregion
		


        #region Property Phase
        private byte _phase = 0;

        /// <summary>
        /// Harmonic Phase (0 -> 127)
        /// </summary>
        public byte Phase
        {
            get
            {
                return _phase;
            }
            set
            {
                Set(ref _phase, value);
            }
        }
        #endregion
		

        /// <summary>
        /// Converts this Midi-based phase & magnitude into a Complex number
        /// </summary>
        /// <param name="type">Type of magnitude</param>
        /// <param name="index">The phase adjustment (typically the harmonic number)</param>
        /// <returns>Complex number</returns>
        public Complex ToComplex(uint index=0, MagnitudeType type=MagnitudeType.Linear)
        {
            // Note that in OscilGen, they divide the phase by the harmonic + 1
            //         hphase[i] = (Phphase[i] - 64.0f) / 64.0f * PI / (i + 1);
            double phase = (Phase - 64.0) / 64.0 * Math.PI / (index + 1.0);

            var magnitudeNew = 1.0 - Math.Abs(Magnitude / 64.0 - 1.0);
            var magnitude = 0.0;
            switch (type)
            {
                case MagnitudeType.dB_40:
                    magnitude = Math.Exp(magnitudeNew * Math.Log(0.01));
                    break;
                case MagnitudeType.dB_60:
                    magnitude = Math.Exp(magnitudeNew * Math.Log(0.001));
                    break;
                case MagnitudeType.dB_80:
                    magnitude = Math.Exp(magnitudeNew * Math.Log(0.0001));
                    break;
                case MagnitudeType.dB_100:
                    magnitude = Math.Exp(magnitudeNew * Math.Log(0.00001));
                    break;
                default:
                    magnitude = 1.0 - magnitudeNew;
                    break;
            }
            if (Magnitude < 64)
                magnitude = -magnitude;
            // If the original magnitude was zero, make sure ours is perfectly 0 too...
            if (Magnitude == 64)
                magnitude = 0.0f;


            return Complex.FromPolarCoordinates(magnitude, phase);
        }

    }
}
