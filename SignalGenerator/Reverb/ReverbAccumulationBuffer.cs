/*
 * Copyright (C) 2010 Google Inc. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1.  Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 * 2.  Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 * 3.  Neither the name of Apple Computer, Inc. ("Apple") nor the names of
 *     its contributors may be used to endorse or promote products derived
 *     from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY APPLE AND ITS CONTRIBUTORS "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL APPLE OR ITS CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using CorySignalGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Reverb
{
    /// <summary>
    /// ReverbAccumulationBuffer is a circular delay buffer with one client reading from it and multiple clients
    /// writing/accumulating to it at different delay offsets from the read position.  The read operation will zero the memory
    /// just read from the buffer, so it will be ready for accumulation the next time around.
    /// </summary>
    public class ReverbAccumulationBuffer
    {
        float[] m_buffer;
        int m_readIndex;
        int m_readTimeFrame; // for debugging (frame on continuous timeline)

        public ReverbAccumulationBuffer(int length)
        {
            m_buffer = new float[length];
            m_readIndex = 0;
            m_readTimeFrame = 0;
        }

        /// <summary>
        /// This will read from, then clear-out numberOfFrames
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="numberOfFrames"></param>
        public void ReadAndClear(float[] destination, int offset, int numberOfFrames)
        {
            var bufferLength = m_buffer.Length;

            bool isCopySafe = m_readIndex <= bufferLength && numberOfFrames <= bufferLength;
            Debug.Assert(isCopySafe);
            if (!isCopySafe)
                return;

            var framesAvailable = bufferLength - m_readIndex;
            var numberOfFrames1 = (int)Math.Min(framesAvailable, numberOfFrames);
            var numberOfFrames2 = numberOfFrames - numberOfFrames1;

            Array.Copy(m_buffer, ReadIndex, destination, offset, numberOfFrames1);
            Array.Clear(m_buffer, ReadIndex, numberOfFrames1);
            if (numberOfFrames2 > 0)
            {
                Array.Copy(m_buffer, 0, destination, offset + numberOfFrames1, numberOfFrames2);
                Array.Clear(m_buffer, 0, numberOfFrames2);
            }
            m_readIndex = (m_readIndex + numberOfFrames) % bufferLength;
            m_readTimeFrame += numberOfFrames;
        }

        /// <summary>
        /// Each ReverbConvolverStage will accumulate its output at the appropriate delay from the read position.
        /// We need to pass in and update readIndex here, since each ReverbConvolverStage may be running in
        /// a different thread than the realtime thread calling ReadAndClear() and maintaining m_readIndex
        /// Returns the writeIndex where the accumulation took place
        /// </summary>
        /// <param name="source"></param>
        /// <param name="numberOfFrames"></param>
        /// <param name="readIndex"></param>
        /// <param name="delayFrames"></param>
        /// <returns></returns>
        public int Accumulate(float[] source, int offset, int numberOfFrames, ref int readIndex, int delayFrames)
        {
            int bufferLength = m_buffer.Length;
            int writeindex = (readIndex + delayFrames) % bufferLength;

            // update callers read index.
            readIndex = (readIndex + numberOfFrames) % bufferLength;


            int framesAvailable = bufferLength - writeindex;
            int numberOfFrames1 = (int)Math.Min(numberOfFrames, framesAvailable);
            int numberOfFrames2 = numberOfFrames - numberOfFrames1;

            bool isSafe = writeindex <= bufferLength && numberOfFrames1 + writeindex <= bufferLength && numberOfFrames2 <= bufferLength;
            Debug.Assert(isSafe);
            if (!isSafe)
                return 0;

            VectorMath.vadd(source, offset, 1, m_buffer, writeindex, 1, m_buffer, writeindex, 1, numberOfFrames1);

            if (numberOfFrames2 > 0)
            {
                VectorMath.vadd(source, numberOfFrames1, 1, m_buffer, 0, 1, m_buffer, 0, 1, numberOfFrames2);
            }

            return writeindex;
        }


        public void UpdateReadIndex(ref int readIndex, int numberOfFrames)
        {
            readIndex = (readIndex + numberOfFrames) % m_buffer.Length;
        }

        public int ReadIndex { get { return m_readIndex; } }

        public int ReadTimeFrame { get { return m_readTimeFrame; } }

        internal void Reset()
        {
            Array.Clear(m_buffer, 0, m_buffer.Length);
            m_readIndex = 0;
            m_readTimeFrame = 0;
        }
    }
}
