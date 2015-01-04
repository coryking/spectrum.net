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

        CircularBuffer inputBuffer;
        CircularBuffer outputBuffer;
        float[] inputSampleBuffer;
        float[] fftSampleBuffer;
        List<float[]> fftInputSampleBuffer;
        float[] fftOutputSampleBuffer;
        bool isEndOfStream; // will be true if source.Read suggests we are done...
        const int SINGLE_BYTES = 4; // four bytes per float

        private const int outputBufferSize = 32768; // 2^15
        private int fftSize;

        // Empirical gain calibration tested across many impulse responses to ensure perceived volume is same as dry (unprocessed) signal
        static readonly double GainCalibration = -58;
        static readonly double GainCalibrationSampleRate = 44100;

        // A minimum power value to when normalizing a silent (or very quiet) impulse response
        static readonly double MinPower = 0.000125f;

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
            outputBuffer = new CircularBuffer(outputBufferSize);
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
            var scale = CalculateNormalizationScale(stream, SampleRate);
            if (WaveFormat.Channels == stream.WaveFormat.Channels)
            {
                for (int i = 0; i < WaveFormat.Channels; i++)
                {
                    convolvers.Add(FFTConvolver.InitFromWaveStream(stream, i, scale));
                }
                fftSize = convolvers[0].FFTSize;
                inputBuffer = new CircularBuffer(fftSize * 2);

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
            {
                samplesWritten += inputBuffer.Write(inputSampleBuffer, samplesRead, ConvolverBlockSize - inputBuffer.Count);
                isEndOfStream = true;
            }

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
            Buffer.BlockCopy(fftSampleBuffer,0, buffer, SINGLE_BYTES * offset, count * SINGLE_BYTES);

            //Array.Copy(fftSampleBuffer, 0, buffer, offset, count);
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
                totalSamplesProcessed += ProcessChannel(fftSampleBuffer, samplesPerChannel, i);
            }
            outputBuffer.Write(fftOutputSampleBuffer, 0, totalSamplesProcessed);
            return totalSamplesProcessed;
        }

        private int ProcessChannel(float[] buffer, int samplesPerChannel,int channel)
        {
            Array.Clear(fftInputSampleBuffer[channel], 0, outputBufferSize);

            var channelBuffer = buffer.TakeChannel(channel, samplesPerChannel, channels: Channels);
            var samplesProcessed = convolvers[channel].Process(channelBuffer, 0, fftInputSampleBuffer[channel], 0, samplesPerChannel);
            fftInputSampleBuffer[channel].InterleaveChannel(fftOutputSampleBuffer, channel, 0, samplesProcessed, channels: Channels);
            return samplesProcessed;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            //FillInputBuffer(count);
            //ProcessFFT();
            while(outputBuffer.Count < count && !isEndOfStream )
            {
                FillInputBuffer(count);
                ProcessFFT();
            }
            int samplesRead;
            // this will probably fail in the case were the input buffer still has data in it...
            if (!isEndOfStream)
            {
                samplesRead= outputBuffer.Read(buffer, offset, count);
            }
            else
            {
                samplesRead= outputBuffer.Read(buffer, offset, (int)Math.Min(count, outputBuffer.Count));
            }
            return samplesRead;
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
            int samples = (int)stream.Length / stream.WaveFormat.BlockAlign;
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
