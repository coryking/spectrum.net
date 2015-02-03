using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Models
{
    /// <summary>
    /// Represents a pitch adjustment
    /// </summary>
    public class PitchAdjustment : PropertyChangeModel
    {
        public PitchAdjustment():base()
        {
            Semitones = 0;
            Cents = 0;
        }

        /// <summary>
        /// Adjust the <paramref name="baseFrequency"/> by the amount given by this class
        /// </summary>
        /// <param name="baseFrequency"></param>
        /// <returns></returns>
        public float AdjustPitch(float baseFrequency)
        {
            return FrequencyUtils.ScaleFrequency(baseFrequency, TotalSemitones, 12f);
        }

        public float TotalSemitones
        {
            get
            {
                return (float)Semitones + Cents / 100.0f;
            }
        }

        #region Property Semitones
        private int _semitones = 0;

        /// <summary>
        /// Gets / Sets the number of semitones
        /// </summary>
        public int Semitones
        {
            get
            {
                return _semitones;
            }
            set
            {
                Set(ref _semitones, value, -12, 12);
            }
        }
        #endregion

        public int TotalCents
        {
            get
            {
                return Semitones * 100 + Cents;
            }
        }

        #region Property Cents
        private int _cents = 0;

        /// <summary>
        ///Gets / Sets the number of cents between each semitone
        /// </summary>
        public int Cents
        {
            get
            {
                return _cents;
            }
            set
            {
                Set(ref _cents, value, -100, 100);
            }
        }
        #endregion

        private IEnumerable<int> _validSemitones = Enumerable.Range(-12, 25);

        /// <summary>
        /// List of valid semitones... used for binding to control boxes
        /// </summary>
        public IEnumerable<int> ValidSemitones
        {
            get { return _validSemitones; }
        }
    }
}
