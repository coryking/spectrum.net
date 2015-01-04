using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Utils
{
    /// <summary>
    /// A very basic circular buffer implementation.
    /// 
    /// Is a generic version of the circular buffer found in naudio
    /// </summary>
    public class CircularBuffer
    {
        private readonly float[] buffer;
        private readonly object lockObject;
        private int writePosition;
        private int readPosition;
        private int byteCount;
        const int SINGLE_BYTES = 4; // four bytes per float


        /// <summary>
        /// Create a new circular buffer
        /// </summary>
        /// <param name="size">Max buffer size in bytes</param>
        public CircularBuffer(int size)
        {
            buffer = new float[size];
            lockObject = new object();
        }

        /// <summary>
        /// Write data to the buffer
        /// </summary>
        /// <param name="data">Data to write</param>
        /// <param name="offset">Offset into data</param>
        /// <param name="count">Number of bytes to write</param>
        /// <returns>number of bytes written</returns>
        public int Write(float[] data, int offset, int count)
        {
            lock (lockObject)
            {
                var bytesWritten = 0;
                if (count > buffer.Length - byteCount)
                {
                    count = buffer.Length - byteCount;
                }
                // write to end
                int writeToEnd = Math.Min(buffer.Length - writePosition, count);
                //Array.Copy(data, offset, buffer, writePosition, writeToEnd);
                Buffer.BlockCopy(data, offset * SINGLE_BYTES, buffer, writePosition * SINGLE_BYTES, writeToEnd * SINGLE_BYTES);
                writePosition += writeToEnd;
                writePosition %= buffer.Length;
                bytesWritten += writeToEnd;
                if (bytesWritten < count)
                {
                    Debug.Assert(writePosition == 0);
                    // must have wrapped round. Write to start
                    Buffer.BlockCopy(
                        data, SINGLE_BYTES * (offset + bytesWritten), buffer, SINGLE_BYTES * writePosition, (count - bytesWritten) * SINGLE_BYTES);

                    //Array.Copy(data, offset + bytesWritten, buffer, writePosition, count - bytesWritten);
                    writePosition += (count - bytesWritten);
                    bytesWritten = count;
                }
                byteCount += bytesWritten;
                return bytesWritten;
            }
        }

        /// <summary>
        /// Read from the buffer
        /// </summary>
        /// <param name="data">Buffer to read into</param>
        /// <param name="offset">Offset into read buffer</param>
        /// <param name="count">Bytes to read</param>
        /// <returns>Number of bytes actually read</returns>
        public int Read(float[] data, int offset, int count)
        {
            lock (lockObject)
            {
                if (count > byteCount)
                {
                    count = byteCount;
                }
                int bytesRead = 0;
                int readToEnd = Math.Min(buffer.Length - readPosition, count);
                Buffer.BlockCopy(buffer, SINGLE_BYTES * readPosition, data, offset * SINGLE_BYTES, readToEnd * SINGLE_BYTES);
                //Array.Copy(buffer, readPosition, data, offset, readToEnd);
                bytesRead += readToEnd;
                readPosition += readToEnd;
                readPosition %= buffer.Length;

                if (bytesRead < count)
                {
                    // must have wrapped round. Read from start
                    Debug.Assert(readPosition == 0);
                    Buffer.BlockCopy(buffer, SINGLE_BYTES * readPosition, data, SINGLE_BYTES * (offset + bytesRead), (count-bytesRead) * SINGLE_BYTES);

                    //Array.Copy(buffer, readPosition, data, offset + bytesRead, count - bytesRead);
                    readPosition += (count - bytesRead);
                    bytesRead = count;
                }

                byteCount -= bytesRead;
                Debug.Assert(byteCount >= 0);
                return bytesRead;
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
        /// Number of bytes currently stored in the circular buffer
        /// </summary>
        public int Count
        {
            get { return byteCount; }
        }

        /// <summary>
        /// Resets the buffer
        /// </summary>
        public void Reset()
        {
            byteCount = 0;
            readPosition = 0;
            writePosition = 0;
        }

        /// <summary>
        /// Advances the buffer, discarding bytes
        /// </summary>
        /// <param name="count">Bytes to advance</param>
        public void Advance(int count)
        {
            if (count >= byteCount)
            {
                Reset();
            }
            else
            {
                byteCount -= count;
                readPosition += count;
                readPosition %= MaxLength;
            }
        }

        public override string ToString()
        {
            return string.Format("CircularBuffer(max: {0}, count: {0})", Count, MaxLength);
        }
    }
}
