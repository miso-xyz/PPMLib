using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PPMLib.Extensions;

namespace PPMLib
{
    public class PPMFile
    {        
        public void LoadFromFile(string path)
        {           
            Parse(File.ReadAllBytes(path));
        }        

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
        }	

		internal static readonly char[] FileMagic = new char[4] { 'P', 'A', 'R', 'A' };
		private uint[] _animationOffset;
		public uint AnimationDataSize { get; private set; }        
        public uint SoundDataSize { get; private set; }
        public ushort FrameCount { get; private set; }
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

	}
}
