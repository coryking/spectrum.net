using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Utils
{
    /// <summary>
    /// A very basic, generic circular buffer implementation
    /// </summary>
    public class CircularBuffer<T>
    {
        private readonly T[] buffer;
        private readonly object lockObject;
        private int readPosition;

        /// <summary>
        /// Create a new circular buffer
        /// </summary>
        /// <param name="size">Max buffer size in bytes</param>
        public CircularBuffer(int size)
        {
            buffer = new T[size];
            lockObject = new object();
        }

        private int computeIndex(int i)
        {
            int index = readPosition;
            index += i;
            index %= MaxLength;
            return index;
        }

        public T this[int index]
        {
            get
            {
                //if (index > buffer.Length)
                //    throw new IndexOutOfRangeException();

                    return buffer[computeIndex(index)];
                
            }
            set
            {
                //if (index > buffer.Length)
                //    throw new IndexOutOfRangeException();

                buffer[computeIndex(index)] = value;

            }
        }

        /// <summary>
        /// Maximum length of this circular buffer
        /// </summary>
        public int MaxLength
        {
            get { return buffer.Length; }
        }


        /// <summary>
        /// Resets the buffer
        /// </summary>
        public void Reset()
        {
            readPosition = 0;
        }

        /// <summary>
        /// Advances the buffer, discarding bytes
        /// </summary>
        /// <param name="count">Bytes to advance</param>
        public void Advance(int count)
        {

            readPosition -= count;
            if (readPosition < 0)
                readPosition = MaxLength - 1;

        }
    }
}
