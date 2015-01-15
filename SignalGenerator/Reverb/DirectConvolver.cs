using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
/*
 * Copyright (C) 2012 Intel Inc. All rights reserved.
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
    public class DirectConvolver
    {
        private int m_inputBlockSize;
        private float[] m_buffer;

        public DirectConvolver(int inputBlockSize)
        {
            m_inputBlockSize = inputBlockSize;
            m_buffer = new float[inputBlockSize*2];
        }

        public void Process(float[] convolutionKernel, float[] source, int sourceOffset, float[] destination, int destinationOffset, int framesToProcess)
        {
            Debug.Assert(framesToProcess == m_inputBlockSize);
            if (framesToProcess != m_inputBlockSize)
                return;

            // Only support kernelSize <= m_inputBlockSize
            int kernelSize = convolutionKernel.Length;
            Debug.Assert(kernelSize <= m_inputBlockSize);
            if (kernelSize > m_inputBlockSize)
                return;

            var bufferOffset = m_inputBlockSize;
            // Copy samples to 2nd half of input buffer.
            Array.Copy(source, sourceOffset, m_buffer, bufferOffset, framesToProcess);

            // If this was C++, we could do cool SSE enabled stuff or use even fancier 
            // CPU functionality.  Instead we are stuck with this crappy nested loop.
            //
            // Boo....
            for (int t = 0; t < framesToProcess; t++)
            {
                float sum = 0f;
                for (int n = 0; n < kernelSize; n++)
                {
                    sum += convolutionKernel[n] * m_buffer[bufferOffset + (t - n)];
                }
                destination[destinationOffset + t] = sum;
            }
            // Copy 2nd half of input buffer to 1st half.
            Array.Copy(m_buffer, bufferOffset, m_buffer, 0, framesToProcess);
            
        }

        public void Reset()
        {
            Array.Clear(m_buffer, 0, m_buffer.Length);
        }

        public override string ToString()
        {
            return String.Format("DirectConvolver: (blocksize: {0}, buffer_len: {1})", m_inputBlockSize, m_buffer.Length);
        }
    }
}
