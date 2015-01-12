using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// From http://blogs.msdn.com/b/pfxteam/archive/2012/03/05/10278165.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            return Task.WhenAll(
                from item in source
                select Task.Run(() => body(item)));
        }

        public static IEnumerable<Complex> ToComplex(this IEnumerable<float> source)
        {
            return source.Select(x =>
            {
                return new Complex
                {
                    X = x,
                    Y = 0
                };
            });
        }

        
        public static Complex[] ToComplexArray(this float[] source)
        {
            return source.ToComplex().ToArray();
        }

        

        /// <summary>
        /// Vector complex multiplication on a complex number.
        /// Taken from http://mxr.mozilla.org/mozilla-central/source/dom/media/webaudio/AudioNodeEngine.cpp
        /// </summary>
        /// <param name="input"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Complex Multiply(this Complex input, Complex scale)
        {
            Complex output;
            output.X = input.X * scale.X - input.Y * scale.Y;
            output.Y = input.X * scale.Y + input.Y * scale.X;
            return output;

        }

        /// <summary>
        /// Vector complex multiplication on an array of complex numbers.
        /// </summary>
        /// <param name="input">input enumerable of complex numbers</param>
        /// <param name="scale">scale enumerable of complex numbers</param>
        public static IEnumerable<Complex> MultiplyComplexNumbers(this IEnumerable<Complex> input, IEnumerable<Complex> scale)
        {
            return input.Zip(scale, (i,s)=>{
                return i.Multiply(s);
            });
        }

        /// <summary>
        /// Returns only the real numbers
        /// </summary>
        /// <param name="input">the input sequence</param>
        /// <param name="offset">the offset in the output buffer</param>
        /// <param name="outputBuffer">the buffer to write into</param>
        /// <returns>how many items were output</returns>
        public static int ToReal(this IList<Complex> input, IList<float> outputBuffer, int offset=0)
        {
            var origOffset = offset;
            for (int i = 0; i < input.Count; i++)
            {
                outputBuffer[offset++] = input[i].X;
            }
            return offset - origOffset;
        }

        public static int ToReal(this IList<System.Numerics.Complex> input, IList<float> outputBuffer, int offset = 0)
        {
            var origOffset = offset;
            for (int i = 0; i < input.Count; i++)
            {
                outputBuffer[offset++] = (float)input[i].Real;
            }
            return offset - origOffset;
        }

        /// <summary>
        /// Add inputBuffer to outputBuffer with a scale
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <param name="scale">How much to scale each item of inputBuffer</param>
        /// <param name="outputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outputOffset"></param>
        public static int AddWithScale(this IList<float> inputBuffer, IList<float> outputBuffer, int size, int outputOffset = 0, float scale=1.0f)
        {
            var origOffset = outputOffset;
            for (int i = 0; i < size; i++)
            {
                outputBuffer[outputOffset] = outputBuffer[outputOffset] + inputBuffer[i] * scale;
                outputOffset++;
            }
            return outputOffset - origOffset;
        }

        /// <summary>
        /// Take a channel from an audio array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="channel">Which channel to take</param>
        /// <param name="count">Number of samples to take</param>
        /// <param name="channels">Number of interleaved channels</param>
        /// <returns></returns>
        public static T[] TakeChannel<T>(this IList<T> input, int channel ,int count, int channels=2)
        {
            var output = new T[count];
            return input.Skip(channel).Where((elem, idx) => idx % channels == 0).Take(count).ToArray();
            
        }

        /// <summary>
        /// Interleave a channel into some buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">source channel</param>
        /// <param name="output">output stream</param>
        /// <param name="channel">which channel the <paramref name="input"/> is</param>
        /// <param name="offset">offset in the <paramref name="output"/> buffer</param>
        /// <param name="count">how many samples to interleave</param>
        /// <param name="channels">total number of channels</param>
        public static void InterleaveChannel<T>(this IList<T> input, IList<T> output, int channel, int offset, int count, int channels=2)
        {
            if (count > input.Count || (count * channel + offset) > output.Count)
                throw new IndexOutOfRangeException("Trying to read more than possible!");

            offset += channel;
            for (int i = 0; i < count; i++)
            {
                output[offset] = input[i];
                offset += channels;
            }

        }

        public static void Scale(this float[] buffer, float scale)
        {
            var bufferSize = buffer.Length;
            for (int i = 0; i < bufferSize; i++)
            {
                buffer[i] *= scale;
            }
        }
        public static void Scale(this IList<float> buffer, int offset, int count, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[i + offset] *= scale;
            }
        }

        public static float SumOfSquares(this IList<float> buffer, int count, int channel, int channels = 2)
        {
            float sum = 0;
            foreach (var item in buffer.TakeChannel(channel, count, channels: channels))
            {
                sum += item * item;
            }
            return sum;
        }
    }
}
