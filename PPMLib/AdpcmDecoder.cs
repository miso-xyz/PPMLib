using PPMLib.Extensions;
using System;

namespace PPMLib
{

    // AdpcmDecoder heavily based on https://github.com/jaames/flipnote.js
    // thank you jaames
    public class AdpcmDecoder
    {
        private int step_index { get; set; }

        private PPMFile Flipnote { get; set; }
        public AdpcmDecoder(PPMFile input)
        {
            this.step_index = 0;
            this.Flipnote = input;
        }

        public int[] IndexTable = new int[16]
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8,
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

        /// <summary>
        /// Get decoded audio track for the BGM. Will be expanded to sound effects soon
        /// </summary>
        /// <returns>Signed 16-Bit PCM audio</returns>
        public short[] Decode()
        {
            var src = Flipnote.Audio.SoundData.RawBGM;
            var srcSize = src.Length;
            var dst = new short[srcSize * 2];
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
                predictor = Utils.NumClamp(predictor, -32768, 32767);

                step_index += IndexTable[sample];
                step_index = Utils.NumClamp(step_index, 0, 88);
                dst[dstPtr++] = (short)predictor;

            }
            
            return dst;
        }

        /// <summary>
        /// Get decoded audio track for the BGM using a specified samplerate. 
        /// could probably work with different samplerates but i don't know why you'd try
        /// </summary>
        /// <param name="dstFreq"></param>
        /// <returns>Signed 16-Bit PCM audio</returns>
        public short[] getAudioTrackPcm(int dstFreq)
        {
            var srcPcm = Decode();
            var srcFreq = 8192.0;
            double soundspeed = Flipnote.BGMRate;
            double framerate = Flipnote.Framerate;

            if(Flipnote.Audio.SoundData.RawBGM.Length != 0)
            {
                
                    
                        var bgmAdjust = (1.0 / soundspeed) / (1.0 / framerate);
                        srcFreq = (8192.0 * bgmAdjust);
                    
                    
                
                
            }
            if((int)srcFreq != dstFreq)
            {
                return pcmResampleNearestNeighbour(srcPcm, srcFreq, dstFreq);
            }
            return srcPcm;
            
        }

        private short[] pcmAudioMix(short[] src, short[] dst, int dstOffset = 0)
        {
            var srcSize = src.Length;
            var dstSize = dst.Length;

            for(int i = 0; i < srcSize; i++)
            {
                if(dstOffset + i > dstSize)
                {
                    break;
                }
                //half src volume
                int samp = 0;
                try
                {
                    samp = dst[dstOffset + i] + (src[i] / 2);
                    dst[dstOffset + i] = (short)Utils.NumClamp(samp, -32768, 32767);
                } catch (Exception e)
                {

                }
                
                
            }
            return dst;
        }


        public short[] getAudioMasterPcm(PPMFile flip, int dstFreq)
        {
            var dstSize = Math.Ceiling((double)timeGetNoteDuration(flip.FrameCount, Flipnote.Framerate) * dstFreq);
            var master = new short[(int)dstSize];
            var bgmPcm = getAudioTrackPcm(dstFreq);
            master = pcmAudioMix(bgmPcm, master, 0);
            return master;
        }

        public int timeGetNoteDuration(int frameCount, double framerate)
        {
            return (int)((frameCount * 100) * (1 / framerate)) / 100;
        }


        private short pcmGetSample(short[] src, int srcSize, int srcPtr)
        {
            if (srcPtr < 0 || srcPtr >= srcSize)
            {
                return 0;
            }
            return src[srcPtr];
        }

        /// <summary>
        /// Zero-order hold (nearest neighbour) audio interpolation.
        /// Credit to SimonTime for the original C version.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcFreq"></param>
        /// <param name="dstFreq"></param>
        /// <returns>Resampled Signed 16-bit PCM audio</returns>
        private short[] pcmResampleNearestNeighbour(short[] src, double srcFreq, int dstFreq)
        {
            var srcLength = src.Length;
            var srcDuration = srcLength / srcFreq;
            var dstLength = srcDuration * dstFreq;
            var dst = new short[(int)dstLength];
            var adjFreq = srcFreq / dstFreq;
            for(var dstPtr = 0; dstPtr < dstLength; dstPtr++)
            {
                dst[dstPtr] = pcmGetSample(src, srcLength, (int)Math.Floor((double)(dstPtr * adjFreq)));
            }
            return dst;
        }

        private short[] pcmResampleLinear(short[] src, double srcFreq, int dstFreq)
        {
            var srcLength = src.Length;
            var srcDuration = srcLength / srcFreq;
            var dstLength = srcDuration * dstFreq;
            var dst = new short[(int)dstLength];
            var adjFreq = srcFreq / dstFreq;

            int adj = 0;
            int srcPtr = 0;
            int weight = 0;

            for (int dstPtr = 0; dstPtr < dstLength; dstPtr++)
            {
                adj = (int)(dstPtr * adjFreq);
                srcPtr = (int)Math.Floor((double)adj);
                weight = adj % 1;
                dst[dstPtr] = (short)((1 - weight) * pcmGetSample(src, srcLength, srcPtr) + weight * pcmGetSample(src, srcLength, srcPtr + 1));
            }
            return dst;
        }
    }
}
