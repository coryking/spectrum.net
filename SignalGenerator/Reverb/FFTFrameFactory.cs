using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Reverb
{
    public static class FFTFrameFactory
    {
        public static IFFTFrame GetNewFFTFrame(int fftSize)
        {
#if USE_FFTW
            return new NativeFFTFrame(fftSize);
#else
            return new FFTFrame(fftSize);
#endif
        }
    }
}
