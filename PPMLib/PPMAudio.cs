using PPMLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PPMLib
{
    public class PPMAudio
    {

        public _SoundHeader SoundHeader { get; set; }
        public _SoundData SoundData { get; set; }

        public PPMAudio()
        {
            //Should probably initialize SoundHeader and Data here, but i'm handling it in flinote loading
            //nobody would ever wanna make a new instance of this right?
        }

        public byte[] GetWavBGM(PPMFile flip, PPMAudioTrack track)
        {
            // start decoding
            AdpcmDecoder encoder = new AdpcmDecoder(flip);
            var decoded = encoder.getAudioTrackPcm(8192 * 2, track);
            byte[] output = new byte[decoded.Length];

            // thank you https://github.com/meemo
            for (int i = 0; i < decoded.Length; i += 2)
            {
                output[i] = (byte)(decoded[i + 1] & 0xff);
                output[i + 1] = (byte)(decoded[i] >> 8);
            }
            
            var wav = new WavePcmFormat(output, 1, 8192, 16);

            return wav.ToBytesArray();

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
