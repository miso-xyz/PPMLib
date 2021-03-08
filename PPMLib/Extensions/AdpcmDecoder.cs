
using System;

namespace PPMLib.Extensions
{

    /// <summary>
    /// AdpcmDecoder heavily based on https://github.com/jaames/flipnote.js
    /// thank you jaames
    /// </summary>
    public class AdpcmDecoder
    {
        private int step_index { get; set; }

        private PPMFile Flipnote { get; set; }

        /// <summary>
        /// Initialize Audio Decoder
        /// </summary>
        /// <param name="input">The Flipnote File as input</param>
        public AdpcmDecoder(PPMFile input)
        {
            this.step_index = 0;
            this.Flipnote = input;
        }

        private int[] IndexTable = new int[16]
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8,
        };

        private int[] ADPCM_STEP_TABLE = new int[]
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
        /// Get decoded audio track for a given track
        /// </summary>
        /// <param name="track">Which type of Audio Track to decode</param>
        /// <returns>Signed 16-Bit PCM audio</returns>
        private short[] Decode(PPMAudioTrack track)
        {
            _SoundData sounds = Flipnote.Audio.SoundData;
            byte[] src = null;
            switch (track)
            {
                case PPMAudioTrack.BGM:
                    src = sounds.RawBGM; break;
                case PPMAudioTrack.SE1:
                    src = sounds.RawSE1; break;
                case PPMAudioTrack.SE2:
                    src = sounds.RawSE2; break;
                case PPMAudioTrack.SE3:
                    src = sounds.RawSE3; break;
                default:
                    src = sounds.RawBGM; break;
            }
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
        /// <param name="track">The type of Audio Track</param>
        /// <returns>Signed 16-Bit PCM audio</returns>
        public short[] getAudioTrackPcm(int dstFreq, PPMAudioTrack track)
        {
            var srcPcm = Decode(track);
            var srcFreq = 8192;
            double soundspeed = Flipnote.BGMRate;
            double framerate = Flipnote.Framerate;

            if (track == PPMAudioTrack.BGM)
            {


                var bgmAdjust = (1.0 / soundspeed) / (1.0 / framerate);
                srcFreq = ((int)(srcFreq * bgmAdjust));




            }
            if ((int)srcFreq != dstFreq)
            {
                return pcmResampleNearestNeighbour(srcPcm, srcFreq, dstFreq);
            }
            return srcPcm;

        }

        /// <summary>
        /// Mixes two tracks together at the given offset
        /// </summary>
        /// <param name="src">The Audio to add</param>
        /// <param name="dst">The output</param>
        /// <param name="dstOffset">The Offset</param>
        /// <returns>Signed 16-bit PCM audio</returns>
        private short[] pcmAudioMix(short[] src, short[] dst, int dstOffset = 0)
        {
            var srcSize = src.Length;
            var dstSize = dst.Length;

            for (int i = 0; i < srcSize; i++)
            {
                if (dstOffset + i > dstSize)
                {
                    break;
                }
                //half src volume
                int samp = 0;
                try
                {
                    samp = dst[dstOffset + i] + (src[i] / 2);
                    dst[dstOffset + i] = (short)Utils.NumClamp(samp, -32768, 32767);
                }
                catch (Exception e)
                {

                }


            }
            return dst;
        }


        /// <summary>
        /// Get the full mixed audio for the Flipnote, using the specified samplerate
        /// </summary>
        /// <param name="flip">The Flipnote</param>
        /// <param name="dstFreq">16384 is recommended</param>
        /// <returns>Signed 16-bit PCM audio</returns>
        public short[] getAudioMasterPcm(int dstFreq)
        {
            var dstSize = (int)Math.Ceiling(timeGetNoteDuration(Flipnote.FrameCount, Flipnote.Framerate) * dstFreq);
            var master = new short[dstSize + 1];
            var hasBgm = Flipnote.Audio.SoundHeader.BGMTrackSize > 0;
            var hasSe1 = Flipnote.Audio.SoundHeader.SE1TrackSize > 0;
            var hasSe2 = Flipnote.Audio.SoundHeader.SE2TrackSize > 0;
            var hasSe3 = Flipnote.Audio.SoundHeader.SE3TrackSize > 0;

            // Mix background music
            if (hasBgm)
            {
                var bgmPcm = getAudioTrackPcm(dstFreq, PPMAudioTrack.BGM);
                master = pcmAudioMix(bgmPcm, master, 0);
            }

            if (hasSe1 || hasSe2 || hasSe3)
            {
                var samplesPerFrame = dstFreq / Flipnote.Framerate;
                var se1Pcm = hasSe1 ? getAudioTrackPcm(dstFreq, PPMAudioTrack.SE1) : null;
                var se2Pcm = hasSe1 ? getAudioTrackPcm(dstFreq, PPMAudioTrack.SE2) : null;
                var se3Pcm = hasSe1 ? getAudioTrackPcm(dstFreq, PPMAudioTrack.SE3) : null;
                var seFlags = Flipnote.SoundEffectFlags;
                for (int i = 0; i < Flipnote.FrameCount; i++)
                {
                    var seOffset = (int)Math.Ceiling(i * samplesPerFrame);
                    var flag = seFlags[i];
                    if (hasSe1 && flag == 1)
                    {
                        master = pcmAudioMix(se1Pcm, master, seOffset);
                    }
                    if (hasSe2 && flag == 2)
                    {
                        master = pcmAudioMix(se2Pcm, master, seOffset);
                    }
                    if (hasSe3 && flag == 4)
                    {
                        master = pcmAudioMix(se3Pcm, master, seOffset);
                    }
                }
            }


            return master;
        }

        /// <summary>
        /// Returns the duration of a Flipnote
        /// </summary>
        /// <param name="frameCount"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public double timeGetNoteDuration(int frameCount, double framerate)
        {
            return ((frameCount * 100) * (1 / framerate)) / 100;
        }

        /// <summary>
        /// Return the sample at the specified position
        /// </summary>
        /// <param name="src">source audio</param>
        /// <param name="srcSize">the size of the source</param>
        /// <param name="srcPtr">the position</param>
        /// <returns></returns>
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
            for (var dstPtr = 0; dstPtr < dstLength; dstPtr++)
            {
                dst[dstPtr] = pcmGetSample(src, srcLength, (int)Math.Floor((double)(dstPtr * adjFreq)));
            }
            return dst;
        }

        /// <summary>
        /// Unused Linear interpolation
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcFreq"></param>
        /// <param name="dstFreq"></param>
        /// <returns>Resampled Signed 16-bit PCM audio</returns>
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
