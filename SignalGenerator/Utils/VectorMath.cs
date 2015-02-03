using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorySignalGenerator.Utils
{
    public static class VectorMath
    {

        public static void vcopy(float[] source, int offset, int sourceStride, float[] destination, int destinationOffset, int destinationStride, int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var sourceP = offset;
            while (n > 0)
            {
                destination[destP] = source[sourceP];
                sourceP += sourceStride;
                destP += destinationStride;
                n--;
            }
        }

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
        public static void vadd(
            float[] source1, int offset1, int sourceStride1, float scale1,
            float[] source2, int offset2, int sourceStride2, float scale2,
            float[] destination, int destinationOffset, int destinationStride,
            int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var source1P = offset1;
            var source2P = offset2;
            while (n > 0)
            {
                destination[destP] = 
                    source1[source1P] * scale1 + 
                    source2[source2P] * scale2;
                source1P += sourceStride1;
                source2P += sourceStride2;
                destP += destinationStride;
                n--;
            }
        }

        public static void vadd(
            float[] source1, int offset1, int sourceStride1,
            float[] source2, int offset2, int sourceStride2,
            float[] source3, int offset3, int sourceStride3,
            float[] destination, int destinationOffset, int destinationStride,

            int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var source1P = offset1;
            var source2P = offset2;
            var source3P = offset3;
            while (n > 0)
            {
                destination[destP] = source1[source1P] + source2[source2P] + source3[source3P];
                source1P += sourceStride1;
                source2P += sourceStride2;
                source3P += sourceStride3; ;
                destP += destinationStride;
                n--;
            }
        }

        public static void vadd(
            float[] source1, int offset1, int sourceStride1, float scale1,
            float[] source2, int offset2, int sourceStride2, float scale2,
            float[] source3, int offset3, int sourceStride3, float scale3,
            float[] destination, int destinationOffset, int destinationStride,

            int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var source1P = offset1;
            var source2P = offset2;
            var source3P = offset3;
            while (n > 0)
            {
                destination[destP] = 
                    source1[source1P] * scale1 + 
                    source2[source2P] * scale2 + 
                    source3[source3P] * scale3;
                source1P += sourceStride1;
                source2P += sourceStride2;
                source3P += sourceStride3; ;
                destP += destinationStride;
                n--;
            }
        }


        public static void vadd(
            float[] source1, int offset1, int sourceStride1,
            float[] source2, int offset2, int sourceStride2,
            float[] source3, int offset3, int sourceStride3,
            float[] source4, int offset4, int sourceStride4,
            float[] destination, int destinationOffset, int destinationStride,

            int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var source1P = offset1;
            var source2P = offset2;
            var source3P = offset3;
            var source4P = offset4;
            while (n > 0)
            {
                destination[destP] = source1[source1P] + source2[source2P] + source3[source3P] + source4[source4P];
                source1P += sourceStride1;
                source2P += sourceStride2;
                source3P += sourceStride3;
                source4P += sourceStride4;
                destP += destinationStride;
                n--;
            }
        }

        public static void vscale(
            float[] source, int offset, int sourceStride, 
            float[] destination, int destinationOffset, int destinationStride, 
            float scale, int framesToProcess)
        {
            var n = framesToProcess;
            var destP = destinationOffset;
            var sourceP = offset;

            while (n > 0)
            {
                destination[destP] = source[sourceP] * scale;
                sourceP += sourceStride;
                destP += destinationStride;
                n--;
            }
        }
    }
}
