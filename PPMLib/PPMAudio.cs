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

        public byte[] GetWavBGM()
        {
            AdpcmDecoder encoder = new AdpcmDecoder();
            byte[] buffer = SoundData.RawBGM;
            var decoded = encoder.Decode(buffer);
            var wav = new WavePcmFormat(decoded, 1, 8192, 8);
            File.WriteAllBytes("bruh.wav", wav.ToBytesArray());
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
