using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class ReverbConvolver
    {
        const int InputBufferSize = 8 * 16384;

        /// <summary>
        /// We only process the leading portion of the impulse response in the real-time thread.  We don't exceed this length.
        /// It turns out then, that the background thread has about 278msec of scheduling slop.
        /// Empirically, this has been found to be a good compromise between giving enough time for scheduling slop,
        /// while still minimizing the amount of processing done in the primary (high-priority) thread.
        /// This was found to be a good value on Mac OS X, and may work well on other platforms as well, assuming
        /// the very rough scheduling latencies are similar on these time-scales.  Of course, this code may need to be
        /// tuned for individual platforms if this assumption is found to be incorrect.
        /// </summary>
        public const int RealtimeFrameLimit = 8192 + 4096; // ~278msec @ 44.1KHz

        public const int MinFFTSize = 128;
        public const int MaxRealtimeFFTSize = 2048;


        List<ReverbConvolverStage> m_stages;
        List<ReverbConvolverStage> m_backgroundStages;

        ReverbAccumulationBuffer m_accumulationBuffer;

        // One or more background threads read from this input buffer which is fed from the realtime thread.
        ReverbInputBuffer m_inputBuffer;

        // First stage will be of size m_minFFTSize.  Each next stage will be twice as big until we hit m_maxFFTSize.
        int m_minFFTSize;
        int m_maxFFTSize;

        // But don't exceed this size in the real-time thread (if we're doing background processing).
        int m_maxRealtimeFFTSize;

        bool m_useBackgroundThreads;

        bool m_wantsToExit;
        bool m_moreInputBuffered;
        object m_backgroundThreadLock = new object();
        Thread m_workerThread;

        // maxFFTSize can be adjusted (from say 2048 to 32768) depending on how much precision is necessary.
        // For certain tweaky de-convolving applications the phase errors add up quickly and lead to non-sensical results with
        // larger FFT sizes and single-precision floats.  In these cases 2048 is a good size.
        // If not doing multi-threaded convolution, then should not go > 8192.
        public ReverbConvolver(IEnumerable<float> impulseResponse, int renderSliceSize, int maxFFTSize, int convolverRenderPhase, bool useBackgroundThreads)
        {

            var totalResponseLength = impulseResponse.Count();

            m_stages=new List<ReverbConvolverStage>();
            m_backgroundStages = new List<ReverbConvolverStage>();
            m_useBackgroundThreads = useBackgroundThreads;
            m_minFFTSize = MinFFTSize;
            m_maxFFTSize = maxFFTSize;
            m_inputBuffer = new ReverbInputBuffer(InputBufferSize);
            m_accumulationBuffer = new ReverbAccumulationBuffer(totalResponseLength + renderSliceSize);
            
            // If we are using background threads then don't exceed this FFT size for the
            // stages which run in the real-time thread.  This avoids having only one or two
            // large stages (size 16384 or so) at the end which take a lot of time every several
            // processing slices.  This way we amortize the cost over more processing slices.
            m_maxRealtimeFFTSize = MaxRealtimeFFTSize;

            // For the moment, a good way to know if we have real-time constraint is to check if we're using background threads.
            // Otherwise, assume we're being run from a command-line tool.
            bool hasRealtimeConstraint = useBackgroundThreads;

            var response = impulseResponse;

            // The total latency is zero because the direct-convolution is used in the leading portion.
            int reverbTotalLatency = 0;

            int stageOffset = 0;
            int i = 0;
            int fftSize = m_minFFTSize;


            while (stageOffset < totalResponseLength)
            {
                int stageSize = fftSize / 2;

                // For the last stage, it's possible that stageOffset is such that we're straddling the end
                // of the impulse response buffer (if we use stageSize), so reduce the last stage's length...
                if (stageSize + stageOffset > totalResponseLength)
                    stageSize = totalResponseLength - stageOffset;
                // This "staggers" the time when each FFT happens so they don't all happen at the same time
                int renderPhase = convolverRenderPhase + i * renderSliceSize;

                bool useDirectConvolver = (stageOffset==0);

                var stage = new ReverbConvolverStage(response, totalResponseLength, reverbTotalLatency, stageOffset, stageSize, fftSize, renderPhase, renderSliceSize, m_accumulationBuffer, useDirectConvolver);

                var isBackgroundStage = false;
                if (useBackgroundThreads && stageOffset > RealtimeFrameLimit)
                {
                    m_backgroundStages.Add(stage);
                    isBackgroundStage = true;
                }
                else
                {
                    m_stages.Add(stage);
                }
                stageOffset += stageSize;
                ++i;

                // Figure out next FFT size
                if (!useDirectConvolver)
                    fftSize *= 2;

                if (hasRealtimeConstraint && !isBackgroundStage && fftSize > m_maxRealtimeFFTSize)
                    fftSize = m_maxRealtimeFFTSize;
                if (fftSize > m_maxFFTSize)
                    fftSize = m_maxFFTSize;

            }
            if (useBackgroundThreads && m_backgroundStages.Count > 0)
            {
                /*m_workerThread = new Thread(new ThreadStart(this.BackgrounThreadEntry))
                {
                    Name="Convolver Thread",
                    Priority = ThreadPriority.Normal,
                    IsBackground=true
                };
                m_workerThread.Start();*/
            }

        }

        public void BackgrounThreadEntry()
        {
            while (!m_wantsToExit)
            {
                m_moreInputBuffered = false;
                lock(m_backgroundThreadLock)
                {
                    while (!m_moreInputBuffered && !m_wantsToExit)
                        Monitor.Wait(m_backgroundThreadLock);

                    this.ProcessInBackground();
                }
            }
        }

        public void Process(float[] sourceChannel, float[] destinationChannel, int framesToProcess)
        {


            bool isSafe = sourceChannel != null & destinationChannel != null && sourceChannel.Length >= framesToProcess && destinationChannel.Length >= framesToProcess;
            Debug.Assert(isSafe);
            if (!isSafe)
                return;

            m_inputBuffer.Write(sourceChannel, framesToProcess);

            foreach (var stage in m_stages)
            {
                stage.Process(sourceChannel, framesToProcess);
            }
            m_accumulationBuffer.ReadAndClear(destinationChannel, framesToProcess);

            // Now that we've buffered more input, post another task to the background thread.
            if (m_useBackgroundThreads && m_backgroundStages.Count > 0)
            {
                if (m_workerThread != null)
                {
                    if (Monitor.TryEnter(m_backgroundThreadLock))
                    {
                        try
                        {
                            m_moreInputBuffered = true;
                            Monitor.Pulse(m_backgroundThreadLock);
                        }
                        finally
                        {
                            Monitor.Exit(m_backgroundThreadLock);
                        }
                    }
                }
                else
                {
                    Task.Run(() =>
                    {
                        this.ProcessInBackground();
                    });
                }
            }
        }

        protected void ProcessInBackground()
        {
            // Process all of the stages until their read indices reach the input buffer's write index
            var writeIndex = m_inputBuffer.WriteIndex;

            // Even though it doesn't seem like every stage needs to maintain its own version of readIndex
            // we do this in case we want to run in more than one background thread.
            int readIndex = 0;

            while ((readIndex = m_backgroundStages[0].InputReadIndex) != writeIndex)
            {
                int SliceSize = MinFFTSize / 2;
                foreach (var backgroundStage in m_backgroundStages)
                {
                    backgroundStage.ProcessInBackground(this, SliceSize);
                }

            }
        }

        public void Reset()
        {
            foreach (var stage in m_stages)
            {
                stage.Reset();
            }
            foreach (var stage in m_backgroundStages)
            {
                stage.Reset();
            }
            m_accumulationBuffer.Reset();
            m_inputBuffer.Reset();
        }

        /// <summary>
        /// Dunno what this is for... but here you go.
        /// </summary>
        public int LatencyFrames { get { return 0; } }

        /// <summary>
        /// Gets if this convolver is using any background processing.
        /// </summary>
        public bool HasBackgroundFrames
        {
            get
            {
                if (m_backgroundStages != null && m_backgroundStages.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public ReverbInputBuffer InputBuffer { get { return m_inputBuffer; } }
    }
}
