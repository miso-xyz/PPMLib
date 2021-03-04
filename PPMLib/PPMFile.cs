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
                Frames[x] = (PPMLib.PPMFrame)ReadPPMFrameData(br);
                Frames[x].AnimationIndex = Array.IndexOf(_animationOffset, _animationOffset[x]);
                if (x > 0)
                {
                    Frames[x].Overwrite(Frames[x - 1]);
                }
            }
        }

		private object ReadPPMFrameData(BinaryReader br)
		{
			//Debug.WriteLine(br.BaseStream.Position.ToString("X8"))
			PPMFrame frame = new PPMFrame();
			frame._firstByteHeader = br.ReadByte();
			if ((frame._firstByteHeader & 0x60) != 0)
			{
				frame._translateX = br.ReadSByte();
				frame._translateY = br.ReadSByte();
			}

			frame.PaperColor = (PPMLib.PaperColor)(frame._firstByteHeader % 2);
			frame.Layer1.PenColor = (PPMLib.PenColor)((frame._firstByteHeader & 0x6) >> 1);
			frame.Layer2.PenColor = (PPMLib.PenColor)((frame._firstByteHeader & 0x18) >> 3);

			frame.Layer1._lineEncoding = br.ReadBytes(0x30);
			frame.Layer2._lineEncoding = br.ReadBytes(0x30);

			for (var line = 0; line <= 191; line++)
			{
				switch (frame.Layer1.LinesEncoding(line))
				{
					case 0:
						break;
					case (LineEncoding)1:
						PPMLineEncDealWith4Bytes(br, frame, 1, line);
						break;
					case (LineEncoding)2:
						PPMLineEncDealWith4Bytes(br, frame, 1, line, true);
						break;
					case (LineEncoding)3:
						PPMLineEncDealWithRawData(br, frame, 1, line);
						break;
				}
			}
			for (var line = 0; line <= 191; line++)
			{
				switch (frame.Layer2.LinesEncoding(line))
				{
					case (LineEncoding)1:
						PPMLineEncDealWith4Bytes(br, frame, 2, line);
						break;
					case (LineEncoding)2:
						PPMLineEncDealWith4Bytes(br, frame, 2, line, true);
						break;
					case (LineEncoding)3:
						PPMLineEncDealWithRawData(br, frame, 2, line);
						break;
				}
			}

			return frame;
		}

		private void PPMLineEncDealWith4Bytes(BinaryReader br, PPMFrame frame, int layer, int line, bool inv = false)
		{
			int x = 0;
			if (inv)
			{
				for (x = 0; x <= 255; x++)
				{
					if (layer == 1)
					{
						frame.Layer1.setPixels(x, line, true);
					}
					else
					{
						frame.Layer2.setPixels(x, line, true);
					}
				}
			}
			x = 0;
			var b1 = br.ReadByte();
			var b2 = br.ReadByte();
			var b3 = br.ReadByte();
			var b4 = br.ReadByte();
			uint bytes = (uint)(((uint)b1 << 24) | ((uint)b2 << 16) | ((uint)b3 << 8) | (uint)b4);
			while (bytes != 0)
			{
				if ((bytes & 0x80000000U) != 0)
				{
					var pixels = br.ReadByte();
					for (int i = 0; i <= 7; i++)
					{
						if (layer == 1)
						{
							var a = ((pixels >> i) & 1) > 0;
							frame.Layer1.setPixels(x, line, a);
							x += 1;
						}
						else
						{
							var a = ((pixels >> i) & 1) > 0;
							frame.Layer2.setPixels(x, line, a);
							x += 1;
						}
					}
				}
				else
				{
					x += 8;
				}
				bytes <<= 1;
			}
		}

		private void PPMLineEncDealWithRawData(BinaryReader br, PPMFrame frame, int layer, int line)
		{
			int y = 0;
			for (var x = 0; x <= 31; x++)
			{
				byte val = br.ReadByte();
				for (var x_ = 0; x_ <= 7; x_++)
				{
					if (layer == 1)
					{
						frame.Layer1.setPixels(y, line, ((((val >> x_) + 1) == 1) ? true : false));
					}
					else
					{
						frame.Layer2.setPixels(y, line, ((((val >> x_) + 1) == 1) ? true : false));
					}
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
