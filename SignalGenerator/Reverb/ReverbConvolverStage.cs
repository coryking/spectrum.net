using CorySignalGenerator.Dsp;
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
    /// A ReverbConvolverStage represents the convolution associated with a sub-section of a large impulse response.
    /// It incorporates a delay line to account for the offset of the sub-section within the larger impulse response.
    /// </summary>
    public class ReverbConvolverStage
    {
        FFTFrame m_fftKernel;
        FFTConvolver m_fftConvolver;

        float[] m_preDelayBuffer;
        ReverbAccumulationBuffer m_accumulationBuffer;

        int m_accumulationReadIndex;
        int m_inputReadIndex;

        int m_preDelayLength;
        int m_postDelayLength;
        int m_preReadWriteIndex;
        int m_framesProcessed;

        int m_stageOffset;
        int m_stageLength;

        float[] m_temporaryBuffer;
        bool m_directMode;

        float[] m_directKernel;
        DirectConvolver m_directConvolver;

        public override string ToString()
        {
            return String.Format("ReverbConvolverStage: (offset: {0}, len: {1}, kernel: {2}, dir:{3})", m_stageOffset, m_stageLength, m_fftConvolver, m_directConvolver);
        }

        // renderPhase is useful to know so that we can manipulate the pre versus post delay so that stages will perform
        // their heavy work (FFT processing) on different slices to balance the load in a real-time thread.
        public ReverbConvolverStage(IEnumerable<float> impulseResponse, int responseLength, int reverbTotalLatency, int stageOffset, int stageLength, int fftSize, int renderPhase, int renderSliceSize, ReverbAccumulationBuffer buffer, bool directMode = false)
        {

            m_stageOffset = stageOffset;
            m_stageLength = stageLength;
            m_accumulationBuffer = buffer;
            m_directMode = directMode;
            if (!m_directMode)
            {
                m_fftKernel = new FFTFrame(fftSize);
                m_fftKernel.DoPaddedFFT(impulseResponse, stageOffset, stageLength);
                m_fftConvolver = new FFTConvolver(fftSize);
            }
            else
            {
                m_directKernel = new float[fftSize / 2];
                Array.Copy(impulseResponse.ToArray(), m_directKernel, stageLength);
                m_directConvolver = new DirectConvolver(renderSliceSize);
            }

            m_temporaryBuffer = new float[renderSliceSize];

            // The convolution stage at offset stageOffset needs to have a corresponding delay to cancel out the offset.
            int totalDelay = stageOffset + reverbTotalLatency;
            // But, the FFT convolution itself incurs fftSize / 2 latency, so subtract this out...
            int halfSize = fftSize / 2;
            if (!m_directMode)
            {
                Debug.Assert(totalDelay >= halfSize);
                if (totalDelay >= halfSize)
                    totalDelay -= halfSize;
            }
    
            // We divide up the total delay, into pre and post delay sections so that we can schedule at exactly the moment when the FFT will happen.
            // This is coordinated with the other stages, so they don't all do their FFTs at the same time...
            int maxPreDelayLength = (int)Math.Min(halfSize, totalDelay);
            m_preDelayLength = totalDelay > 0 ? renderPhase % maxPreDelayLength : 0;
            if (m_preDelayLength > totalDelay)
                m_preDelayLength = 0;

            m_postDelayLength = totalDelay - m_preDelayLength;
            m_preReadWriteIndex = 0;
            m_framesProcessed = 0; // total frames processed so far....

            int delayBufferSize = m_preDelayLength < fftSize ? fftSize : m_preDelayLength;
            delayBufferSize = delayBufferSize < renderSliceSize ? renderSliceSize : delayBufferSize;

            m_preDelayBuffer = new float[delayBufferSize];
        }

        /// <summary>
        // WARNING: framesToProcess must be such that it evenly divides the delay buffer size (stage_offset).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="framesToProcess"></param>
        public void Process(float[] source, int framesToProcess)
        {
            Debug.Assert(source != null && source.Length > 0);
            if (source == null || source.Length == 0)
                return;

            float[] preDelayedSource;
            int preDelayedSourceOffset = 0;
            float[] preDelayedDestination;
            int preDelayedDestinationOffset = 0;
            float[] temporaryBuffer;
            int temporaryBufferOffset = 0;
            bool isTemporaryBufferSafe = false;
            if (m_preDelayLength > 0)
            {
                bool isPreDelaySafe = m_preReadWriteIndex + framesToProcess < m_preDelayBuffer.Length;
                Debug.Assert(isPreDelaySafe);
                if (!isPreDelaySafe)
                    return;

                isTemporaryBufferSafe = framesToProcess <= m_temporaryBuffer.Length;

                preDelayedDestination = m_preDelayBuffer;
                preDelayedDestinationOffset = m_preReadWriteIndex;
                preDelayedSource = preDelayedDestination;
                preDelayedSourceOffset = preDelayedDestinationOffset;
                temporaryBuffer = m_temporaryBuffer;
            }
            else
            {
                // Zero delay
                preDelayedDestination = null;
                preDelayedSource = source;
                temporaryBuffer = m_preDelayBuffer;
                isTemporaryBufferSafe = framesToProcess <= m_preDelayBuffer.Length;
            }

            Debug.Assert(isTemporaryBufferSafe);
            if (!isTemporaryBufferSafe)
                return;

            if (m_framesProcessed < m_preDelayLength)
            {
                // For the first m_preDelayLength frames don't process the convolver, instead simply buffer in the pre-delay.
                // But while buffering the pre-delay, we still need to update our index.
                m_accumulationBuffer.UpdateReadIndex(ref m_accumulationReadIndex, framesToProcess);
            }
            else
            {
                if (m_directMode)
                    m_directConvolver.Process(m_directKernel, preDelayedSource, preDelayedSourceOffset, temporaryBuffer, temporaryBufferOffset, framesToProcess);
                else
                    // Now, run the convolution (into the delay buffer).
                    // An expensive FFT will happen every fftSize / 2 frames.
                    // We process in-place here...
                    m_fftConvolver.Process(m_fftKernel, preDelayedSource, preDelayedSourceOffset, temporaryBuffer, temporaryBufferOffset, framesToProcess);

                // Now accumulate into reverb's accumulation buffer.
                m_accumulationBuffer.Accumulate(temporaryBuffer, temporaryBufferOffset, framesToProcess, ref m_accumulationReadIndex, m_postDelayLength);
            }

            if (m_preDelayLength > 0)
            {
                Array.Copy(source, 0, preDelayedDestination, preDelayedDestinationOffset, framesToProcess);
                m_preReadWriteIndex += framesToProcess;

                Debug.Assert(m_preReadWriteIndex <= m_preDelayLength);
                if (m_preReadWriteIndex >= m_preDelayLength)
                    m_preReadWriteIndex = 0;
            }

            m_framesProcessed += framesToProcess;
        }


        public void ProcessInBackground(ReverbConvolver convolver, int framesToProcess)
        {
            ReverbInputBuffer inputBuffer = convolver.InputBuffer;
            var source = inputBuffer.DirectReadFrom(ref m_inputReadIndex, framesToProcess);
            Process(source, framesToProcess);
        }
        public void Reset()
        {
            if (!m_directMode)
                m_fftConvolver.Reset();
            else
                m_directConvolver.Reset();

            Array.Clear(m_preDelayBuffer, 0, m_preDelayBuffer.Length);
            m_accumulationReadIndex = 0;
            m_inputReadIndex = 0;
            m_framesProcessed = 0;
        }

        public int InputReadIndex { get { return m_inputReadIndex; } }
    }
}
