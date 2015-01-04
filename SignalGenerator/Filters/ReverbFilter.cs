using CorySignalGenerator.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorySignalGenerator.Extensions;
using CorySignalGenerator.Utils;

namespace CorySignalGenerator.Filters
{
    public class ReverbFilter : Effect
    {
        List<FFTConvolver> convolvers = new List<FFTConvolver>();

        CircularBuffer<float> inputBuffer;
        CircularBuffer<float> outputBuffer;
        float[] inputSampleBuffer;
        float[] fftSampleBuffer;
        List<float[]> fftInputSampleBuffer;
        float[] fftOutputSampleBuffer;

        private const int outputBufferSize = 32768; // 2^15
        private int fftSize;

        /// <summary>
        /// The total number of samples needed before doing an FFT convolution
        /// </summary>
        private int ConvolverBlockSize
        {
            get
            {
                return fftSize * WaveFormat.Channels;
            }
        }

        public ReverbFilter(ISampleProvider source) : base(source)
        {

        }
        protected override void Init()
        {
            base.Init();
            inputSampleBuffer = new float[outputBufferSize];
            fftSampleBuffer = new float[outputBufferSize];
            fftOutputSampleBuffer = new float[outputBufferSize];
            outputBuffer = new CircularBuffer<float>(outputBufferSize);
            fftInputSampleBuffer = new List<float[]>();

            for (int i = 0; i < WaveFormat.Channels; i++)
			{
                fftInputSampleBuffer.Add(new float[outputBufferSize]);
			}
        }

        public void LoadImpuseResponseWaveStream(WaveStream stream)
        {
            if (WaveFormat.SampleRate != stream.WaveFormat.SampleRate)
                throw new InvalidOperationException("Different sample rates!");

            if (WaveFormat.Channels == stream.WaveFormat.Channels)
            {
                for (int i = 0; i < WaveFormat.Channels; i++)
                {
                    convolvers.Add(FFTConvolver.InitFromWaveStream(stream, i));
                }
                fftSize = convolvers[0].FFTSize;
                inputBuffer = new CircularBuffer<float>(fftSize * 2);

            }
            else
            {
                throw new InvalidOperationException("Channel mismatch!");
            }
           
        }

        private int FillInputBuffer(int count)
        {
            Array.Clear(inputSampleBuffer, 0, inputSampleBuffer.Length);
            int samplesWritten = 0;
            var samplesRead = Source.Read(inputSampleBuffer, 0, count);
            samplesWritten += inputBuffer.Write(inputSampleBuffer, 0, samplesRead);
            // we are at the end of some stream.... we need to pad our output buffer so it is a multiple of fftSize
            if (samplesRead < count)
                samplesWritten += inputBuffer.Write(inputSampleBuffer, samplesRead, ConvolverBlockSize - inputBuffer.Count);

            return samplesWritten;
        }

        /// <summary>
        /// FIll buffer with zeros
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int ReadZeros(float[] buffer, int offset, int count)
        {
            Array.Clear(fftSampleBuffer, 0, fftSampleBuffer.Length);
            Array.Copy(fftSampleBuffer, 0, buffer, offset, count);
            return count;
        }

        private int ProcessFFT()
        {
            if (inputBuffer.Count < ConvolverBlockSize)
                return 0;

            Array.Clear(fftSampleBuffer, 0, fftSampleBuffer.Length);
            Array.Clear(fftOutputSampleBuffer, 0, outputBufferSize);
            var samplesRead = inputBuffer.Read(fftSampleBuffer, 0, ConvolverBlockSize);
            if (samplesRead != ConvolverBlockSize)
                throw new InvalidOperationException("Something happened.  the number of samples read from the input buffer were not what was expected!");

            var samplesPerChannel = samplesRead / Channels;
            var totalSamplesProcessed = 0;
            for (int i = 0; i < WaveFormat.Channels; i++)
            {
                totalSamplesProcessed += ProcessChannel(samplesPerChannel, i);
            }
            outputBuffer.Write(fftOutputSampleBuffer, 0, totalSamplesProcessed);
            return totalSamplesProcessed;
        }

        private int ProcessChannel(int samplesPerChannel,int channel)
        {
            Array.Clear(fftInputSampleBuffer[channel], 0, outputBufferSize);

            var channelBuffer = fftSampleBuffer.TakeChannel(channel, samplesPerChannel, channels: Channels);
            var samplesProcessed = convolvers[channel].Process(channelBuffer, 0, fftInputSampleBuffer[channel], 0, channelBuffer.Length);
            fftInputSampleBuffer[channel].InterleaveChannel(fftOutputSampleBuffer, channel, 0, samplesProcessed, channels: Channels);
            return samplesProcessed;
        }

        public override int Read(float[] buffer, int offset, int count)
        {

            var samplesRead = FillInputBuffer(count);
            ProcessFFT();

            return outputBuffer.Read(buffer, offset, (int)Math.Min(count, outputBuffer.Count));

        }
    }
}
