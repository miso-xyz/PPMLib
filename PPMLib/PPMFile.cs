using PPMLib.Extensions;
using System;
using System.IO;
using System.Linq;

namespace PPMLib
{
    public class PPMFile
    {
        /// <summary>
        /// Read file as a flipnote
        /// </summary>
        /// <param name="path">Path to Flipnote</param>
        public void LoadFrom(string path) //private static
        {
            Parse(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Parse a flipnote's raw bytes
        /// </summary>
        /// <param name="bytes">Raw Flipnote Bytes</param>
        public void Parse(byte[] bytes)
        {
            var br = new BinaryReader(new MemoryStream(bytes));
            if (!br.ReadChars(4).SequenceEqual(FileMagic))
            {
                throw new FileFormatException("Unexpected file format");
            }
            AnimationDataSize = br.ReadUInt32();
            SoundDataSize = br.ReadUInt32();
            FrameCount = (ushort)(br.ReadUInt16() + 1);
            FormatVersion = br.ReadUInt16();
            IsLocked = br.ReadUInt16() != 0;
            ThumbnailFrameIndex = br.ReadUInt16();
            string rootname = br.ReadWChars(11);
            string parentname = br.ReadWChars(11);
            string currentname = br.ReadWChars(11);
            ulong parentid = br.ReadUInt64();
            ulong currentid = br.ReadUInt64();
            ParentFilename = br.ReadPPMFilename();
            CurrentFilename = br.ReadPPMFilename();
            ulong rootid = br.ReadUInt64();
            RootAuthor = new PPMAuthor(rootname, rootid);
            ParentAuthor = new PPMAuthor(parentname, parentid);
            CurrentAuthor = new PPMAuthor(currentname, currentid);
            RootFileFragment = br.ReadPPMFileFragment();
            Timestamp = br.ReadPPMTimestamp();
            br.ReadUInt16(); // 0x9E
            Thumbnail = br.ReadPPMThumbnail();
            FrameOffsetTableSize = br.ReadUInt16();
            br.ReadUInt32(); //0x6A2 - always 0
            AnimationFlags = br.ReadUInt16();
            var oCnt = FrameOffsetTableSize / 4.0 - 1;
            _animationOffset = new uint[(int)oCnt + 1];
            Frames = new PPMLib.PPMFrame[FrameCount];
            for (var x = 0; x <= oCnt; x++)
            {
                _animationOffset[x] = br.ReadUInt32();
            }
            long framePos0 = br.BaseStream.Position;
            var offset = framePos0; //&H6A8 + FrameOffsetTableSize
            for (var x = 0; x <= oCnt; x++)
            {
                if (offset + _animationOffset[x] == 4288480943)
                {
                    throw new Exception("Data corrupted (possible memory pit?)");
                }
                br.BaseStream.Seek(offset + _animationOffset[x], SeekOrigin.Begin);
                Frames[x] = br.ReadPPMFrame();
                Frames[x].AnimationIndex = Array.IndexOf(_animationOffset, _animationOffset[x]);
                if (x > 0)
                {
                    Frames[x].Overwrite(Frames[x - 1]);
                }
            }

            // Read Sound Data
            if (SoundDataSize == 0) return;

            offset = 0x6A0 + AnimationDataSize;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            SoundEffectFlags = new byte[Frames.Length];
            Audio = new PPMAudio();
            for (int i = 0; i < Frames.Length; i++)
            {
                SoundEffectFlags[i] = br.ReadByte();
            }
            offset += Frames.Length;

            // make the next offset dividable by 4
            br.ReadBytes((int)((4 - offset % 4) % 4));

            Audio.SoundData = new _SoundData();
            Audio.SoundHeader = new _SoundHeader();

            Audio.SoundHeader.BGMTrackSize = br.ReadUInt32();
            Audio.SoundHeader.SE1TrackSize = br.ReadUInt32();
            Audio.SoundHeader.SE2TrackSize = br.ReadUInt32();
            Audio.SoundHeader.SE3TrackSize = br.ReadUInt32();
            Audio.SoundHeader.CurrentFramespeed = (byte)(8 - br.ReadByte());
            Audio.SoundHeader.RecordingBGMFramespeed = (byte)(8 - br.ReadByte());

            // 
            Framerate = PPM_FRAMERATES[Audio.SoundHeader.CurrentFramespeed];
            BGMRate = PPM_FRAMERATES[Audio.SoundHeader.RecordingBGMFramespeed];
            br.ReadBytes(14);

            Audio.SoundData.RawBGM = br.ReadBytes((int)Audio.SoundHeader.BGMTrackSize);
            Audio.SoundData.RawSE1 = br.ReadBytes((int)Audio.SoundHeader.SE1TrackSize);
            Audio.SoundData.RawSE2 = br.ReadBytes((int)Audio.SoundHeader.SE2TrackSize);
            Audio.SoundData.RawSE3 = br.ReadBytes((int)Audio.SoundHeader.SE3TrackSize);

            // Read Signature (Will implement later)
            if (br.BaseStream.Position == br.BaseStream.Length)
            {
                // file is RSA unsigned -> do something...
            }
            else
            {
                // Next 0x80 bytes = RSA-1024 SHA-1 signature
                Signature = br.ReadBytes(0x80);
                var padding = br.ReadBytes(0x10);
                // Next 0x10 bytes are filled with 0
            }

        }

        internal static readonly char[] FileMagic = new char[4] { 'P', 'A', 'R', 'A' };
        private uint[] _animationOffset;
        public uint AnimationDataSize { get; private set; }
        public uint SoundDataSize { get; private set; }
        public ushort FrameCount { get; private set; } // we should just use Frames.Count() instead
        public ushort FormatVersion { get; private set; }
        public bool IsLocked { get; private set; }
        public ushort ThumbnailFrameIndex { get; private set; }
        public PPMAuthor RootAuthor { get; private set; }
        public PPMAuthor ParentAuthor { get; private set; }
        public PPMAuthor CurrentAuthor { get; private set; }
        public PPMFilename ParentFilename { get; private set; }
        public PPMFilename CurrentFilename { get; private set; }
        public PPMFileFragment RootFileFragment { get; private set; }
        public PPMTimestamp Timestamp { get; private set; }
        public PPMThumbnail Thumbnail { get; private set; }
        public ushort FrameOffsetTableSize { get; private set; }
        public ushort AnimationFlags { get; private set; }
        public PPMFrame[] Frames { get; private set; }
        public byte[] SoundEffectFlags;
        public PPMAudio Audio { get; private set; }
        public byte[] Signature;
        public double Framerate { get; private set; }
        public double BGMRate { get; private set; }

        public double[] PPM_FRAMERATES = new double[]
        {
            30.0,
            0.5,
            1.0,
            2.0,
            4.0,
            6.0,
            12.0,
            20.0,
            30.0
        };

    }
}
