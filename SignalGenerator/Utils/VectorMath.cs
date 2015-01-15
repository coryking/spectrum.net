using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Utils
{
    public static class VectorMath
    {
        public static void vadd(float[] source1, float[] source2, float[] destrination, int framesToProcess)
        {
            vadd(source1, 0, 1, source2, 0, 1, destrination, 0, 1, framesToProcess);
        }

        public static void vadd(
            float[] source1, int offset1, int sourceStride1, 
            float[] source2, int offset2, int sourceStride2, 
            float[] destination, int destinationOffset, int destinationStride, 
            int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var source1P = offset1;
            var source2P = offset2;
            while (n > 0)
            {
                destination[destP] = source1[source1P] + source2[source2P];
                source1P += sourceStride1;
                source2P += sourceStride2;
                destP += destinationStride;
                n--;
            }
        }

    }
}
