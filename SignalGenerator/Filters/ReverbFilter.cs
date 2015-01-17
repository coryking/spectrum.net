using CorySignalGenerator.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
using CorySignalGenerator.Utils;
using CorySignalGenerator.Reverb;
using System.Diagnostics;
using CorySignalGenerator.SampleProviders;

namespace CorySignalGenerator.Filters
{
    public class ReverbFilter : Effect
    {
        public static readonly int MaxFrameSize = 256;
        public static readonly int ProcessingSizeInFrames = 128;
        List<ReverbConvolver> m_convolvers;


        // Empirical gain calibration tested across many impulse responses to ensure perceived volume is same as dry (unprocessed) signal
        static readonly double GainCalibration = -58;
        static readonly double GainCalibrationSampleRate = 44100;

        // A minimum power value to when normalizing a silent (or very quiet) impulse response
        static readonly double MinPower = 0.000125f;

        
        float m_impulseResponseLength;

        int m_maxFFTSize;
        bool m_useBackgroundThreads;
    
        // renderSliceSize is a rendering hint, so the FFTs can be optimized to not all occur at the same time (very bad when rendering on a real-time thread).
        int m_renderSliceSize;

        ReadRateChangeProvider m_rateChanger;
        FuncSampleProvider m_funcProvider;

        public ReverbFilter(ISampleProvider source, int maxFFTSize, bool useBackgroundThreads) : base(source)
        {

            m_convolvers = new List<ReverbConvolver>();
            m_renderSliceSize = ProcessingSizeInFrames;
            m_maxFFTSize = maxFFTSize;
            m_useBackgroundThreads = useBackgroundThreads;
            m_funcProvider = new FuncSampleProvider(source.WaveFormat)
            {
                Function = this.ProcessSample
            };
            m_rateChanger = new ReadRateChangeProvider(m_funcProvider, ProcessingSizeInFrames * source.WaveFormat.Channels);
        }


        protected override void Init()
        {
            base.Init();



        }

        /// <summary>
        /// Loads up an impulse response from a WaveStream
        /// </summary>
        /// <param name="impulseResponseStream"></param>
        public void LoadImpuseResponseWaveStream(WaveStream impulseResponseStream)
        {
            if (WaveFormat.SampleRate != impulseResponseStream.WaveFormat.SampleRate)
                throw new InvalidOperationException("Different sample rates!");
            var scale = CalculateNormalizationScale(impulseResponseStream, SampleRate);
            impulseResponseStream.Seek(0, System.IO.SeekOrigin.Begin);
            var sourceStream = impulseResponseStream.ToSampleProvider();
            int sourceStreamLen = (int)impulseResponseStream.WaveFormat.Channels * (int)impulseResponseStream.Length / impulseResponseStream.BlockAlign;
            var sourceBuffer = new float[sourceStreamLen];
            var count = sourceStream.Read(sourceBuffer, 0, sourceStreamLen);

            var convolverRenderPhase = 0;

            for (int i = 0; i < impulseResponseStream.WaveFormat.Channels; i++)
            {
                var outBuffer = sourceBuffer.TakeChannel(i, sourceStreamLen / impulseResponseStream.WaveFormat.Channels).ToArray();
                outBuffer.Scale(scale);
                m_convolvers.Add(new ReverbConvolver(outBuffer, m_renderSliceSize, m_maxFFTSize, convolverRenderPhase, m_useBackgroundThreads));
                m_impulseResponseLength = outBuffer.Length;
                convolverRenderPhase += m_renderSliceSize;

            }

        }

        private int ProcessSample(float[] buffer, int offset, int count)
        {
            // Do a fairly comprehensive sanity check.
            // If these conditions are satisfied, all of the source and destination pointers will be valid for the various matrixing cases.
            bool isSafeToProcess = count <= MaxFrameSize * Channels;
            Debug.Assert(isSafeToProcess);
            if (!isSafeToProcess)
                return 0;

            float[] sourceBuffer = new float[count];
            var sourceSize = Source.Read(sourceBuffer, 0, count);
            var numReverbChannels = m_convolvers.Count;

            if (Channels == 2 && numReverbChannels == 2)
            {
                var framesToProcess = count / 2;
                var leftBuffer = new float[framesToProcess];
                var rightBuffer = new float[framesToProcess];
                m_convolvers[0].Process(sourceBuffer.TakeChannel(0, framesToProcess).ToArray(),  leftBuffer, framesToProcess);
                m_convolvers[1].Process(sourceBuffer.TakeChannel(1, framesToProcess).ToArray(),  rightBuffer, framesToProcess);

                leftBuffer.InterleaveChannel(buffer, 0, 0, framesToProcess);
                rightBuffer.InterleaveChannel(buffer, 1, 0, framesToProcess);
                buffer.Scale(offset, count, 100);
                return count;
            }
            else
            {
                Array.Copy(sourceBuffer, 0, buffer, offset, sourceSize);
                return sourceSize;
            }
        }
       

        /// <summary>
        /// This read method will always return something even if the source material has long ago stopped producing samples...
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected override int OnRead(float[] buffer, int offset, int count)
        {
            return m_rateChanger.Read(buffer, offset, count);
        }

        /// <summary>
        /// Taken from https://code.google.com/p/chromium/codesearch#chromium/src/third_party/WebKit/Source/platform/audio/Reverb.cpp
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public static float CalculateNormalizationScale(WaveStream stream, int sampleRate)
        {
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            int bufferlen = 2048 * stream.WaveFormat.Channels;
            int samples = (int)stream.WaveFormat.Channels * (int)stream.Length / stream.WaveFormat.BlockAlign;
            samples = samples % 2 == 0 ? samples : samples + 1;
            var buffer = new float[bufferlen];
            var sampleProvider = stream.ToSampleProvider();
            var count = sampleProvider.Read(buffer,0,bufferlen);
            double power = 0;

            while(count > 0){
                for (int i = 0; i < stream.WaveFormat.Channels; i++)
                {
                    var channelPower = buffer.SumOfSquares(bufferlen / stream.WaveFormat.Channels, i, stream.WaveFormat.Channels);
                    power += channelPower;
                }
                count = sampleProvider.Read(buffer, 0, bufferlen);
            }

            power = (float)Math.Sqrt(power / (stream.WaveFormat.Channels * samples));
            power = (float)Math.Min(power, MinPower);

            var scale = 1 / power;
            scale *= Math.Pow(10, GainCalibration * 0.05); // calibrate to make perceived volume same as unprocessed

            // Scale depends on sample-rate.
            scale *= GainCalibrationSampleRate / sampleRate;

            // Scale depends on channels
            if (stream.WaveFormat.Channels == 4)
                scale *= 0.5f;
            return (float)scale;

        }
    }
}
