using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace CorySignalGenerator.Reverb
{
    /// <summary>
    /// ReverbInputBuffer is used to buffer input samples for deferred processing by the background threads.
    /// </summary>
    public class ReverbInputBuffer
    {
        int m_writeIndex;
        float[] m_buffer;

        public ReverbInputBuffer(int length)
        {
            m_buffer = new float[length];
            m_writeIndex = 0;
        }

        /// <summary>
        /// The realtime audio thread keeps writing samples here.
        /// The assumption is that the buffer's length is evenly divisible by numberOfFrames (for nearly all cases this will be fine).
        /// FIXME: remove numberOfFrames restriction...
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="numberOfFrames"></param>
        public void Write(float[] source, int numberOfFrames)
        {
            var bufferLength = m_buffer.Length;
            bool isCopySafe = m_writeIndex + numberOfFrames <= bufferLength;
            Debug.Assert(isCopySafe);
            if (!isCopySafe)
                return;

            Array.Copy(source, 0, m_buffer, m_writeIndex, numberOfFrames);

            m_writeIndex += numberOfFrames;
            Debug.Assert(m_writeIndex <= bufferLength);

            if (m_writeIndex >= bufferLength)
                m_writeIndex = 0;
        }

        /// <summary>
        /// Background threads can call this to check if there's anything to read...
        /// </summary>
        public int WriteIndex { get { return m_writeIndex; } }

        /// <summary>
        /// The individual background threads read here (and hope that they can keep up with the buffer writing).
        /// readIndex is updated with the next readIndex to read from...
        /// The assumption is that the buffer's length is evenly divisible by numberOfFrames.
        /// FIXME: remove numberOfFrames restriction...
        /// </summary>
        /// <param name="readIndex"></param>
        /// <param name="numberOfFrames"></param>
        /// <returns></returns>
        public float[] DirectReadFrom(ref int readIndex, int numberOfFrames)
        {
            var bufferLength = m_buffer.Length;
            bool isPointerGood = readIndex >= 0 && readIndex + numberOfFrames <= bufferLength;
            Debug.Assert(isPointerGood);
            if (!isPointerGood)
            {
                readIndex = 0;
                return m_buffer;
            }
            // the original code returned a pointer to the offset of m_buffer...
            // can't return pointers in c# so we return a copy instead...
            var outBuffer = new float[numberOfFrames];
            Array.Copy(m_buffer, readIndex, outBuffer, 0, numberOfFrames);
            //var result = m_buffer.Skip(readIndex).Take(numberOfFrames);
            readIndex = (readIndex + numberOfFrames) % bufferLength;
            return outBuffer; // result.ToArray();

        }
        public void Reset()
        {
            Array.Clear(m_buffer, 0, m_buffer.Length);
            m_writeIndex = 0;
        }
    }
}
