﻿using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using System.Collections.Concurrent;
using CorySignalGenerator.Utils;

namespace CorySignalGenerator.SampleProviders
{
    public struct MultiThreadedReadSample
    {
        public int samplesRead;
        public float[] samples;
        public ISampleProvider provider;
    }

    /// <summary>
    /// A sample provider mixer, allowing inputs to be added and removed.
    /// 
    /// Unlike NAudio's implementation, this one exposes all the sources.
    /// </summary>
    public class ExposedMixingSampleProvider : ISampleProvider
    {
        private List<ISampleProvider> sources;
        private WaveFormat waveFormat;
        private float[] sourceBuffer;
        private const int maxInputs = 1024; // protect ourselves against doing something silly
        private bool useThreadedMixer = true;
        /// <summary>
        /// Creates a new MixingSampleProvider, with no inputs, but a specified WaveFormat
        /// </summary>
        /// <param name="waveFormat">The WaveFormat of this mixer. All inputs must be in this format</param>
        public ExposedMixingSampleProvider(WaveFormat waveFormat, bool threaded=true)
        {
            if (waveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                throw new ArgumentException("Mixer wave format must be IEEE float");
            }
            this.sources = new List<ISampleProvider>();
            this.waveFormat = waveFormat;
            this.useThreadedMixer = threaded;
        }

        /// <summary>
        /// Creates a new MixingSampleProvider, based on the given inputs
        /// </summary>
        /// <param name="sources">Mixer inputs - must all have the same waveformat, and must
        /// all be of the same WaveFormat. There must be at least one input</param>
        public ExposedMixingSampleProvider(IEnumerable<ISampleProvider> sources)
        {
            this.sources = new List<ISampleProvider>();
            foreach (var source in sources)
            {
                AddMixerInput(source);
            }
            if (this.sources.Count == 0)
            {
                throw new ArgumentException("Must provide at least one input in this constructor");
            }
        }

        /// <summary>
        /// A enumerable list of all the sources this knows about.
        /// </summary>
        public IEnumerable<ISampleProvider> Sources { get { return this.sources; } }

        /// <summary>
        /// When set to true, the Read method always returns the number
        /// of samples requested, even if there are no inputs, or if the
        /// current inputs reach their end. Setting this to true effectively
        /// makes this a never-ending sample provider, so take care if you plan
        /// to write it out to a file.
        /// </summary>
        public bool ReadFully { get; set; }

       

        /// <summary>
        /// Adds a new mixer input
        /// </summary>
        /// <param name="mixerInput">Mixer input</param>
        public void AddMixerInput(ISampleProvider mixerInput)
        {
            // we'll just call the lock around add since we are protecting against an AddMixerInput at
            // the same time as a Read, rather than two AddMixerInput calls at the same time
            lock (sources)
            {
                if (this.sources.Count >= maxInputs)
                {
                    throw new InvalidOperationException("Too many mixer inputs");
                }
                this.sources.Add(mixerInput);
            }
            if (this.waveFormat == null)
            {
                this.waveFormat = mixerInput.WaveFormat;
            }
            else
            {
                if (this.WaveFormat.SampleRate != mixerInput.WaveFormat.SampleRate ||
                    this.WaveFormat.Channels != mixerInput.WaveFormat.Channels)
                {
                    throw new ArgumentException("All mixer inputs must have the same WaveFormat");
                }
            }
        }

        /// <summary>
        /// Removes a mixer input
        /// </summary>
        /// <param name="mixerInput">Mixer input to remove</param>
        public void RemoveMixerInput(ISampleProvider mixerInput)
        {
            lock (sources)
            {
                this.sources.Remove(mixerInput);
            }
        }

        /// <summary>
        /// Removes all mixer inputs
        /// </summary>
        public void RemoveAllMixerInputs()
        {
            lock (sources)
            {
                this.sources.Clear();
            }
        }

        /// <summary>
        /// The output WaveFormat of this sample provider
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return this.waveFormat; }
        }

        /// <summary>
        /// Reads samples from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int outputSamples = 0;
            this.sourceBuffer = BufferHelpers.Ensure(this.sourceBuffer, count);
            lock (sources)
            {
                if (useThreadedMixer)
                    outputSamples = MultiThreadedRead(buffer, offset, count);
                else
                    outputSamples = SingleThreadedRead(buffer, offset, count);
            }
            // optionally ensure we return a full buffer
            if (ReadFully && outputSamples < count)
            {
                int outputIndex = offset + outputSamples;
                while (outputIndex < offset + count)
                {
                    buffer[outputIndex++] = 0;
                }
                outputSamples = count;
            }
            return outputSamples;
        }

        private int MultiThreadedRead(float[] buffer, int offset, int count)
        {
            var outputSamples = 0;
            var items = new ConcurrentQueue<MultiThreadedReadSample>();
            Array.Clear(buffer, offset, count);
            Parallel.ForEach(sources, (item) =>
            {
                var read = new MultiThreadedReadSample()
                {
                    provider = item,
                    samples = new float[count]
                };
                read.samplesRead = item.Read(read.samples, 0, count);
                items.Enqueue(read);
            });
            while (!items.IsEmpty) {
                var sample = new MultiThreadedReadSample();
                items.TryDequeue(out sample);

                outputSamples = Math.Max(sample.samplesRead, outputSamples);

                if (sample.samplesRead == 0)
                    sources.Remove(sample.provider);

                VectorMath.vadd(sample.samples, 0, 1, buffer, offset, 1, buffer, offset, 1, sample.samplesRead);
            }

            return outputSamples;
        }

        private int SingleThreadedRead(float[] buffer, int offset, int count)
        {
            var outputSamples = 0;

            int index = sources.Count - 1;
            while (index >= 0)
            {
                var source = sources[index];
                int samplesRead = source.Read(this.sourceBuffer, 0, count);
                int outIndex = offset;
                for (int n = 0; n < samplesRead; n++)
                {
                    if (n >= outputSamples)
                    {
                        buffer[outIndex++] = this.sourceBuffer[n];
                    }
                    else
                    {
                        buffer[outIndex++] += this.sourceBuffer[n];
                    }
                }
                outputSamples = Math.Max(samplesRead, outputSamples);
                if (samplesRead == 0)
                {
                    sources.RemoveAt(index);
                }
                index--;
            }
            return outputSamples;
        }

    }
}
