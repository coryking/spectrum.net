using NAudio.Dsp;
using System;
using System.Collections.Generic;
using MoreLinq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
using NAudio.Wave;
using System.Diagnostics;
using CorySignalGenerator.Utils;

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
    public class FFTConvolver
    {
        private int m_fftSize;

        private FFTFrame m_frame;

        float[] m_inputBuffer;
        float[] m_outputBuffer;
        float[] m_lastOverlapBuffer;
        int m_readWriteIndex = 0;

        public FFTConvolver(int fftSize)
        {
#if USE_MKL
            MathNet.Numerics.Control.UseNativeMKL();
#endif
            
            // TODO: Complete member initialization
            this.m_fftSize = fftSize;
            this.m_frame = new FFTFrame(fftSize);
            m_inputBuffer = new float[fftSize];
            m_outputBuffer = new float[fftSize];
            m_lastOverlapBuffer = new float[fftSize / 2];
        }
        
        public void Process(FFTFrame fftKernel, float[] source, int sourceOffset, float[] destination, int destinationOffset, int framesToProcess)
        {
            int halfSize = FFTSize / 2;
            // framesToProcess must be an exact multiple of halfSize,
            // or halfSize is a multiple of framesToProcess when halfSize > framesToProcess.
            bool isGood = !(halfSize % framesToProcess !=0 && framesToProcess % halfSize !=0);
            Debug.Assert(isGood);
            if (!isGood)
                return;

            int numberOfDivisions = halfSize < framesToProcess ? (framesToProcess / halfSize) : 1;
            int divisionSize = numberOfDivisions == 1 ? framesToProcess : halfSize;

            var sourceP = sourceOffset;
            var destP = destinationOffset;
            for (int i = 0; i < numberOfDivisions; i++, sourceP += divisionSize, destP+=divisionSize)
            {
                // Copy samples to input buffer (note contraint above!)
                var inputP = m_inputBuffer;
                bool isCopyGood1 = m_readWriteIndex + divisionSize <= m_inputBuffer.Length;
                Debug.Assert(isCopyGood1,"isCopyGood1");
                if (!isCopyGood1)
                    return;

                Array.Copy(source, sourceP, inputP, m_readWriteIndex, divisionSize);


                // Copy samples from output buffer
                var outputP = m_outputBuffer;
                bool isCopyGood2 = m_readWriteIndex + divisionSize <= m_outputBuffer.Length;
                Debug.Assert(isCopyGood2, "isCopyGood2");
                if (!isCopyGood2)
                    return;

                Array.Copy(outputP, m_readWriteIndex, destination, destP, divisionSize);
                m_readWriteIndex += divisionSize;
                if (m_readWriteIndex == halfSize)
                {
                    m_frame.FFTConvolve(fftKernel, m_inputBuffer, m_outputBuffer, 0);
                    VectorMath.vadd(m_outputBuffer, 0, 1, m_lastOverlapBuffer, 0, 1, m_outputBuffer, 0, 1, halfSize);

                    bool isCopyGood3 = m_outputBuffer.Length == 2 * halfSize && m_lastOverlapBuffer.Length == halfSize;
                    Debug.Assert(isCopyGood3, "isCopyGood3");
                    if (!isCopyGood3)
                        return;

                    Array.Copy(m_outputBuffer, halfSize, m_lastOverlapBuffer, 0, halfSize);
                    m_readWriteIndex = 0;
                        
                }
            }
        }

        public int FFTSize { get { return m_fftSize; } }

        public void Reset()
        {
            Array.Clear(m_lastOverlapBuffer, 0, m_lastOverlapBuffer.Length);
            m_readWriteIndex = 0;
        }

        public override string ToString()
        {
            return String.Format("FFTConvolver: (fftsize: {0}, halfsize: {1})", FFTSize, FFTSize / 2);
        }
    }

}
