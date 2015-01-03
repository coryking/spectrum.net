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
        public static int ToReal(this IEnumerable<Complex> input, IList<float> outputBuffer, int offset=0)
        {
            var origOffset = offset;
            foreach (var item in input)
            {
                outputBuffer[offset++] = item.X;
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

        public static T[] TakeChannel<T>(this IList<T> input, int channel,int channels=2)
        {
            var outSize = Convert.ToInt32(input.Count / channels); ;
            var output = new T[outSize];
            return input.Skip(channel).Where((elem, idx) => idx % channels == 0).ToArray();
            
        }

        public static void Interleave<T>(this IList<T> input, IList<T> output, int every, int offset)
        {
            foreach (var item in input)
            {
                output[offset] = item;
                offset += every;
            }
        }

    }
}
