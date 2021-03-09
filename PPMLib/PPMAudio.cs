
using PPMLib.Extensions;
using System;
using System.IO;

namespace PPMLib
{
    public class PPMAudio
    {

        public _SoundHeader SoundHeader { get; set; }
        public _SoundData SoundData { get; set; }


        public PPMAudio()
        {

        }

        /// <summary>
        /// Returns the fully mixed audio of the Flipnote, Including its Sound Effects.
        /// Returns Null if no audio exists.
        /// </summary>
        /// <param name="flip"></param>
        /// <param name="sampleRate"></param>
        /// <returns>Signed 16-bit PCM audio</returns>
        public byte[] GetWavBGM(PPMFile flip, int sampleRate = 32768)
        {
            // start decoding
            AdpcmDecoder encoder = new AdpcmDecoder(flip);
            var decoded = encoder.getAudioMasterPcm(sampleRate);
            if(decoded.Length > 0)
            {
                byte[] output = new byte[decoded.Length];

                // thank you https://github.com/meemo
                for (int i = 0; i < decoded.Length; i += 2)
                {
                    try
                    {
                        output[i] = (byte)(decoded[i + 1] & 0xff);
                        output[i + 1] = (byte)(decoded[i] >> 8);
                    }
                    catch(Exception e)
                    {

                    }
                    
                }
                var a = new WavePcmFormat(output, 1, (uint)(sampleRate / 2), 16);
                return a.ToBytesArray();
            }
            return null;
        }

    }

    public class _SoundHeader
    {
        public uint BGMTrackSize;
        public uint SE1TrackSize;
        public uint SE2TrackSize;
        public uint SE3TrackSize;
        public byte CurrentFramespeed;
        public byte RecordingBGMFramespeed;
    }

    public class _SoundData
    {
        public byte[] RawBGM;
        public byte[] RawSE1;
        public byte[] RawSE2;
        public byte[] RawSE3;
    }
}
