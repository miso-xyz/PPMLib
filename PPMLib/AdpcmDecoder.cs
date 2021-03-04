using PPMLib.Extensions;
using System;

namespace PPMLib
{
    public class AdpcmDecoder
    {
        private int prev_sample { get; set; }
        private int step_index { get; set; }
        public AdpcmDecoder()
        {
            this.prev_sample = 0;
            this.step_index = 0;
        }

        public int[] IndexTable = new int[16]
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8,
        };

        public int[] StepTable = new int[89]
        {
            7,     8,     9,    10,    11,    12,    13,    14,    16,    17,
            19,    21,    23,    25,    28,    31,    34,    37,    41,    45,
            50,    55,    60,    66,    73,    80,    88,    97,   107,   118,
            130,   143,   157,   173,   190,   209,   230,   253,   279,   307,
            337,   371,   408,   449,   494,   544,   598,   658,   724,   796,
            876,   963,  1060,  1166,  1282,  1411,  1552,  1707,  1878,  2066,
            2272,  2499,  2749,  3024,  3327,  3660,  4026,  4428,  4871,  5358,
            5894,  6484,  7132,  7845,  8630,  9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        public int[] ADPCM_STEP_TABLE = new int[]
        {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767, 0
        };

        static int swapNibbles(int x)
        {
            return ((x & 0x0F) << 4 |
                    (x & 0xF0) >> 4);
        }

        public byte[] Decode(byte[] track)
        {

            //sample = (byte)swapNibbles(sample);

            //int delta = sample - prev_sample;
            //int enc_sample = 0;

            //if (delta < 0)
            //{
            //    enc_sample = 8;
            //    delta = -delta;
            //}

            //enc_sample += Math.Min(7, (delta * 4 / StepTable[step_index]));
            //prev_sample = sample;
            //step_index = Utils.Clamp(step_index + IndexTable[enc_sample], 0, 79);

            //return enc_sample;

            var src = track;
            var srcSize = src.Length;
            var dst = new byte[srcSize * 2];
            var srcPtr = 0;
            var dstPtr = 0;
            var sample = 0;
            step_index = 0;
            var predictor = 0;
            var lowNibble = true;

            while (srcPtr < srcSize)
            {
                // switch between high and low nibble each loop iteration
                // increments srcPtr after every high nibble
                if (lowNibble)
                {
                    sample = src[srcPtr] & 0xF;
                }
                else
                {
                    sample = src[srcPtr++] >> 4;
                }
                lowNibble = !lowNibble;
                var step = ADPCM_STEP_TABLE[step_index];
                var diff = step >> 3;

                if ((sample & 1) != 0)
                {
                    diff += step >> 2;
                }
                if ((sample & 2) != 0)
                {
                    diff += step >> 1;
                }
                if ((sample & 4) != 0)
                {
                    diff += step;
                }
                if ((sample & 8) != 0)
                {
                    diff = -diff;
                }
                predictor += diff;
                predictor = Utils.Clamp(predictor, -32768, 32767);
                step_index += ADPCM_STEP_TABLE[sample];
                step_index = Utils.Clamp(step_index, 0, 88);
                dst[dstPtr++] = (byte)predictor;

            }
            return dst;
        }

        private byte pcmGetSample(byte[] src, int srcSize, int srcPtr)
        {
            if (srcPtr < 0 || srcPtr >= srcSize)
            {
                return 0;
            }
            return src[srcPtr];
        }

        private byte[] pcmResampleNearestNeighbour(byte[] src, int srcFreq, int dstFreq)
        {
            throw new NotImplementedException();
        }
    }
}
