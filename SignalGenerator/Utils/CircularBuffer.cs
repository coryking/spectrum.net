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
        /// <param name="srcBuffer">Data to write</param>
        /// <param name="offset">Offset into data</param>
        /// <param name="count">Number of bytes to write</param>
        /// <returns>number of bytes written</returns>
        public int Write(float[] srcBuffer, int offset, int count)
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
                if (writeToEnd < 0) // if this thing gets so hosed that this is negative... we need to just start over...
                    Reset();
                //Array.Copy(data, offset, buffer, writePosition, writeToEnd);
                Buffer.BlockCopy(srcBuffer, offset * SINGLE_BYTES, buffer, writePosition * SINGLE_BYTES, writeToEnd * SINGLE_BYTES);
                writePosition += writeToEnd;
                writePosition %= buffer.Length;
                bytesWritten += writeToEnd;
                if (bytesWritten < count)
                {
                    Debug.Assert(writePosition == 0);
                    // must have wrapped round. Write to start
                    Buffer.BlockCopy(
                        srcBuffer, SINGLE_BYTES * (offset + bytesWritten), buffer, SINGLE_BYTES * writePosition, (count - bytesWritten) * SINGLE_BYTES);

                    //Array.Copy(data, offset + bytesWritten, buffer, writePosition, count - bytesWritten);
                    writePosition += (count - bytesWritten);
                    bytesWritten = count;
                }
                byteCount += bytesWritten;
                return bytesWritten;
            }
        }

        /// <summary>
        /// Add buffer to circular buffer data, optionally advancing the write position
        /// </summary>
        /// <param name="srcBuffer">Data to write</param>
        /// <param name="offset">Offset into data</param>
        /// <param name="count">Number of bytes to write</param>
        /// <param name="advance">If true, will advance the write position</param>
        /// <returns></returns>
        public int Accumulate(float[] srcBuffer, int offset, int count, bool advance)
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
                if (writeToEnd < 0)
                    Reset();
                //Buffer.BlockCopy(srcBuffer, offset * SINGLE_BYTES, buffer, writePosition * SINGLE_BYTES, writeToEnd * SINGLE_BYTES);
                VectorMath.vadd(srcBuffer, offset, 1, buffer, writePosition, 1, buffer, writePosition, 1, writeToEnd);
                var newWritePosition = writePosition + writeToEnd;
                newWritePosition %= buffer.Length;
                bytesWritten += writeToEnd;
                if (bytesWritten < count)
                {
                    Debug.Assert(newWritePosition == 0);
                    // must have wrapped round. Write to start
                    //Buffer.BlockCopy(
                    //    srcBuffer, SINGLE_BYTES * (offset + bytesWritten), buffer, SINGLE_BYTES * newWritePosition, (count - bytesWritten) * SINGLE_BYTES);
                    VectorMath.vadd(srcBuffer, offset + bytesWritten, 1, buffer, newWritePosition, 1, buffer, newWritePosition, 1, count-bytesWritten);

                    //Array.Copy(data, offset + bytesWritten, buffer, writePosition, count - bytesWritten);
                    newWritePosition += (count - bytesWritten);

                    bytesWritten = count;
                }

                if (advance)
                {
                    byteCount += bytesWritten;
                    writePosition = newWritePosition;
                }
                
                return bytesWritten;
            }
        }

        /// <summary>
        /// Read from the buffer
        /// </summary>
        /// <param name="dstBuffer">Buffer to read into</param>
        /// <param name="offset">Offset into read buffer</param>
        /// <param name="count">Bytes to read</param>
        /// <param name="clearbuffer">If true, will clear the previously read segment.</param>
        /// <returns>Number of bytes actually read</returns>
        public int Read(float[] dstBuffer, int offset, int count, bool clearbuffer=true)
        {
            lock (lockObject)
            {
                if (count > byteCount)
                {
                    count = byteCount;
                }
                int bytesRead = 0;
                int readToEnd = Math.Min(buffer.Length - readPosition, count);
                if (readToEnd > 0)
                {
                    Buffer.BlockCopy(buffer, SINGLE_BYTES * readPosition, dstBuffer, offset * SINGLE_BYTES, readToEnd * SINGLE_BYTES);
                    if(clearbuffer)
                        Array.Clear(buffer, readPosition, readToEnd);
                }
                //Array.Copy(buffer, readPosition, data, offset, readToEnd);
                bytesRead += readToEnd;
                readPosition += readToEnd;
                readPosition %= buffer.Length;

                if (bytesRead < count)
                {
                    // must have wrapped round. Read from start
                    Debug.Assert(readPosition == 0);
                    Buffer.BlockCopy(buffer, SINGLE_BYTES * readPosition, dstBuffer, SINGLE_BYTES * (offset + bytesRead), (count-bytesRead) * SINGLE_BYTES);
                    if (clearbuffer)
                        Array.Clear(buffer, readPosition, (count - bytesRead));
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
            Array.Clear(buffer, 0, buffer.Length);
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
