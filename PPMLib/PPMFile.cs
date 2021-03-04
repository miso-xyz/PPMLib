//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace PPMLib
{
	public class PPMFile
	{
		private byte[] ppm;
		private uint[] _animationOffset;
		public PPMFile(string path)
		{
			ppm = File.ReadAllBytes(path);
			Load();
		}
		public PPMFile(byte[] input)
		{
			ppm = input;
			Load();
		}
		private byte[] GetBytes(int offset, int count)
		{
			byte[] b_ = null;
			using (MemoryStream a_ = new MemoryStream())
			{
				a_.Write(ppm, offset, count);
				b_ = a_.ToArray();
			}
			return b_;
		}
		private object ReadWChars(BinaryReader br, int count)
		{
			return Encoding.Unicode.GetString(br.ReadBytes(2 * count));
		}
		public void Load()
		{
			using (BinaryReader br = new BinaryReader(new MemoryStream(ppm)))
			{
				if (br.ReadChars(4).Equals("PARA"))
				{
					throw new FileFormatException("Unexpected file format.");
				}
				_animationDataSize = br.ReadUInt32();
				SoundDataSize = br.ReadUInt32();
				FramesCount = (UInt16)(br.ReadUInt16() + 1);
				if (br.ReadUInt16() != 0x24)
				{
					throw new FileFormatException("Wrong format version. It should be 0x24.");
				}
				Editable = br.ReadUInt16() != 0;
				ThumbnailFrameIndex = br.ReadUInt16();
				RootAuthor = Convert.ToString(ReadWChars(br, 11));
				ParentAuthor = Convert.ToString(ReadWChars(br, 11));
				CurrentAuthor = Convert.ToString(ReadWChars(br, 11));
				ParentAuthorID = br.ReadBytes(8);
				CurrentAuthorID = br.ReadBytes(8);
				ParentFilename = new Filename18(br.ReadBytes(18));
				CurrentFilename = new Filename18(br.ReadBytes(18));
				RootAuthorID = br.ReadBytes(8);
				RootFilenameFragment = Encoding.Default.GetString(br.ReadBytes(8));
				Timestamp = br.ReadUInt32();
				br.ReadUInt16(); //Metadata._0x9E =
				_rawThumbnail = br.ReadBytes(1536);
				FrameOffsetTableSize = br.ReadUInt16();
				br.ReadUInt32(); //0x6A2 - always 0
				AnimationFlags = br.ReadUInt16();
				var oCnt = FrameOffsetTableSize / 4.0 - 1;
				_animationOffset = new uint[(int)oCnt + 1];
				_frames = new PPMLib.PPMFrame[FramesCount];
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
				return;
				//
				// Sound Decoding (WIP)
				//
				if (SoundDataSize == 0)
				{
					return;
				}
				br.ReadBytes((int)Math.Truncate((double)(4 - offset % 4) % 4));
				_bgmtracksize = br.ReadUInt32();
				_se1tracksize = br.ReadUInt32();
				_se2tracksize = br.ReadUInt32();
				_se3tracksize = br.ReadUInt32();
				_currentFrameSpeed = br.ReadByte();
				_bgmFrameSpeed = br.ReadByte();
				br.ReadBytes(14);
				BGMTrack = br.ReadBytes((int)_bgmtracksize);
				SE1Track = br.ReadBytes((int)_se1tracksize);
				SE2Track = br.ReadBytes((int)_se2tracksize);
				SE3Track = br.ReadBytes((int)_se3tracksize);
				if (br.BaseStream.Position == br.BaseStream.Length)
				{
					// unsigned
				}
				else
				{
					// signed
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

		//todo: create `PPMFrame.PaperColorToGDIColor()` and use this

		//public WriteableBitmap ExportFrame(int frameIndex)
		//{
		//	BitmapPalette palette = new BitmapPalette(new List<Color>() {Frames[frameIndex].PaperColorToGDIColor(), PenColorToGDIColor(Frames[frameIndex].PaperColor, Frames[frameIndex].Layer1.PenColor), PenColorToGDIColor(Frames[frameIndex].PaperColor, Frames[frameIndex].Layer2.PenColor)}); // From {Frames(frameIndex).PaperColor, Frames(frameIndex).Frame1Color, Frames(frameIndex).Frame2Color}
		//	WriteableBitmap bmp = new WriteableBitmap(256, 192, 96, 96, System.Windows.Media.PixelFormats.Indexed2, palette);
		//	byte[] pixels = new byte[(64 * 192) + 1];
		//	for (var x = 0; x <= 256; x++)
		//	{
		//		for (var y = 0; y <= 192; y++)
		//		{
		//			if (Frames[frameIndex].Layer2.Pixels(y, x))
		//			{
		//				var b = 256 * y + x;
		//				var p = 3 - b % 4;
		//				b = b / 4;
		//				pixels[b] += (byte)~(0x11 << (2 * p));
		//				pixels[b] = (byte)(pixels[b] | (byte)(0x10 << (2 - p)));
		//			}
		//			if (Frames[frameIndex].Layer1.Pixels(y, x))
		//			{
		//				var b = 256 * y + x;
		//				var p = 3 - b % 4;
		//				b = b / 4;
		//				pixels[b] += (byte)~(0x11 << (2 * p));
		//				pixels[b] = (byte)(pixels[b] | (byte)(0x1 << (2 - p)));
		//			}
		//		}
		//	}
		//	bmp.WritePixels(new System.Windows.Int32Rect(0, 0, 256, 192), pixels, 64, 0);
		//	return bmp;
		//}


		public object SignFlipnote(string key)
		{
			key.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace(System.Environment.NewLine, "");
			var rsa = CreateRsaProviderFromPrivateKey(key);
			var hash = (new SHA1CryptoServiceProvider()).ComputeHash(Encoding.Default.GetBytes(key));
			return rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
		}

		public bool VerifyKey(string key)
		{
			key.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace(System.Environment.NewLine, "");
			using (var md5_ = MD5.Create())
			{
				if (Encoding.Default.GetString(md5_.ComputeHash(Encoding.Default.GetBytes(key))) != "7a38d03a22c7e8b50f67028afafce3cb")
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		private RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string key)
		{
			byte[] privatekey_bytes = Convert.FromBase64String(key);
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			RSAParameters RSAparams = new RSAParameters();
			using (BinaryReader br = new BinaryReader(new MemoryStream(privatekey_bytes)))
			{
				byte bt = 0;
				ushort twobytes = br.ReadUInt16();
				switch (twobytes)
				{
					case 0x8130:
						br.ReadByte();
						break;
					case 0x8230:
						br.ReadInt16();
						break;
					default:
						throw new Exception("Unexpected format! - Expected: 0x8130, 0x8230 - Received: " + twobytes);
				}
				twobytes = br.ReadUInt16();
				if (twobytes != 0x102)
				{
					throw new Exception("Unexpected version!");
				}
				bt = br.ReadByte();
				if (bt != 0x0)
				{
					throw new Exception("Unexpected format");
				}
				RSAparams.Modulus = br.ReadBytes(GetIntegerSize(br));
				RSAparams.Exponent = br.ReadBytes(GetIntegerSize(br));
				RSAparams.D = br.ReadBytes(GetIntegerSize(br));
				RSAparams.P = br.ReadBytes(GetIntegerSize(br));
				RSAparams.Q = br.ReadBytes(GetIntegerSize(br));
				RSAparams.DP = br.ReadBytes(GetIntegerSize(br));
				RSAparams.DQ = br.ReadBytes(GetIntegerSize(br));
				RSAparams.InverseQ = br.ReadBytes(GetIntegerSize(br));
			}
			rsa.ImportParameters(RSAparams);
			return rsa;
		}

		public int GetIntegerSize(BinaryReader r)
		{
			byte bt = r.ReadByte();
			byte lowbyte = 0x0;
			byte highbyte = 0x0;
			int count = 0;
			if (bt != 0x2)
			{
				return 0;
			}
			bt = r.ReadByte();
			switch (bt)
			{
				case 0x81:
					count = r.ReadByte();
					break;
				case 0x82:
					highbyte = r.ReadByte();
					lowbyte = r.ReadByte();
					count = BitConverter.ToInt32(new byte[] {lowbyte, highbyte, 0x0, 0x0}, 0);
					break;
				default:
					count = bt;
					break;
			}
			while (r.ReadByte() == 0x0)
			{
				count -= 1;
			}
			r.BaseStream.Seek(-1, SeekOrigin.Current);
			return count;
		}

		public int PlaybackSpeedToMS()
		{
			switch (8 - PlaybackSpeed)
			{
				case 1:
					return 2000;
				case 2:
					return 1000;
				case 3:
					return 500;
				case 4:
					return 250;
				case 5:
					return 166;
				case 6:
					return 83;
				case 7:
					return 50;
				case 8:
					return 33;
				default:
					return 33;
			}
		}
		public Color PenColorToGDIColor(PaperColor paperColor, PenColor penColor)
		{
			switch (penColor)
			{
				case PPMLib.PenColor.Blue:
					return ColorTranslator.FromHtml("#0a39ff");
				case PPMLib.PenColor.Red:
					return ColorTranslator.FromHtml("#ff2a2a");
				case PPMLib.PenColor.Inverted:
					if (paperColor == PPMLib.PaperColor.Black)
					{
						return Color.White;
					}
					else
					{
						return ColorTranslator.FromHtml("#0e0e0e");
					}
					break;
			}
//INSTANT C# NOTE: Inserted the following 'return' since all code paths must return a value in C#:
			return new Color();
		}
#region Properties
		private string _currentAuthor;
		private string _rootAuthor;
		private string _parentAuthor;
		private byte[] _parentAuthorID;
		private byte[] _currentAuthorID;
		private byte[] _rootAuthorID;
		private UInt16 _frameIndex;
		private UInt16 _totalFrames;
		private bool _editable;
		private UInt16 _thumbnailFrameIndex;
		private Filename18 _parentFilename;
		private Filename18 _currentFilename;
		private string _rootFilenameFragment;
		private UInt32 _timeStamp;
		private bool _loop;
		private int _playbackSpeed;
		private byte[] _rawThumbnail;
		private UInt16 _frameOffsetTableSize;
		private UInt16 _animationFlags;
		private PPMFrame[] _frames;
		private UInt32 _bgmtracksize;
		private UInt32 _se1tracksize;
		private UInt32 _se2tracksize;
		private UInt32 _se3tracksize;
		private byte _currentFrameSpeed;
		private byte _bgmFrameSpeed;
		private byte[] _rawBGM;
		private byte[] _rawSE1;
		private byte[] _rawSE2;
		private byte[] _rawSE3;
		private UInt32 _soundDataSize;
		private byte[] _soundEffectFlags;
		private byte[] _signature = new byte[129];
		private UInt32 _animationDataSize;

		public byte[] Signature
		{
			get
			{
				return _signature;
			}
		}
		public UInt32 BGMTrackSize
		{
			get
			{
				return _bgmtracksize;
			}
		}
		public UInt32 SE1TrackSize
		{
			get
			{
				return _se1tracksize;
			}
		}
		public UInt32 SE2TrackSize
		{
			get
			{
				return _se2tracksize;
			}
		}
		public UInt32 SE3TrackSize
		{
			get
			{
				return _se3tracksize;
			}
		}
		public byte[] SoundEffectFlags
		{
			get
			{
				return _soundEffectFlags;
			}
			set
			{
				_soundEffectFlags = value;
			}
		}

		public UInt32 SoundDataSize
		{
			get
			{
				return _soundDataSize;
			}
			set
			{
				_soundDataSize = value;
			}
		}

		public byte[] BGMTrack
		{
			get
			{
				return _rawBGM;
			}
			set
			{
				_rawBGM = value;
			}
		}
		public byte[] SE1Track
		{
			get
			{
				return _rawSE1;
			}
			set
			{
				_rawSE1 = value;
			}
		}
		public byte[] SE2Track
		{
			get
			{
				return _rawSE2;
			}
			set
			{
				_rawSE2 = value;
			}
		}
		public byte[] SE3Track
		{
			get
			{
				return _rawSE3;
			}
			set
			{
				_rawSE3 = value;
			}
		}
		public PPMFrame[] Frames
		{
			get
			{
				return _frames;
			}
			set
			{
				_frames = value;
			}
		}

		[TypeConverter(typeof(U16HexStringConverter))]
		public UInt16 AnimationFlags
		{
			get
			{
				return _animationFlags;
			}
			set
			{
				_animationFlags = value;
			}
		}

		public UInt16 FrameOffsetTableSize
		{
			get
			{
				return _frameOffsetTableSize;
			}
			set
			{
				_frameOffsetTableSize = value;
			}
		}

		public string CurrentAuthor
		{
			get
			{
				return _currentAuthor;
			}
			set
			{
				_currentAuthor = value;
			}
		}
		public string RootAuthor
		{
			get
			{
				return _rootAuthor;
			}
			set
			{
				_rootAuthor = value;
			}
		}
		public string ParentAuthor
		{
			get
			{
				return _parentAuthor;
			}
			set
			{
				_parentAuthor = value;
			}
		}
		public byte[] ParentAuthorID
		{
			get
			{
				return _parentAuthorID;
			}
			set
			{
				_parentAuthorID = value;
			}
		}
		public byte[] CurrentAuthorID
		{
			get
			{
				return _currentAuthorID;
			}
			set
			{
				_currentAuthorID = value;
			}
		}
		public byte[] RootAuthorID
		{
			get
			{
				return _rootAuthorID;
			}
			set
			{
				_rootAuthorID = value;
			}
		}
		public UInt16 FrameIndex
		{
			get
			{
				return _frameIndex;
			}
			set
			{
				_frameIndex = value;
			}
		}

		public byte[] Thumbnail
		{
			get
			{
				return _rawThumbnail;
			}
		}

		public bool Editable
		{
			get
			{
				return _editable;
			}
			set
			{
				_editable = value;
			}
		}
		public UInt16 FramesCount
		{
			get
			{
				return _totalFrames;
			}
			set
			{
				_totalFrames = value;
			}
		}
		public UInt16 ThumbnailFrameIndex
		{
			get
			{
				return _thumbnailFrameIndex;
			}
			set
			{
				_thumbnailFrameIndex = value;
			}
		}

		[TypeConverter(typeof(Filename18StringConverter))]
		public Filename18 ParentFilename
		{
			get
			{
				return _parentFilename;
			}
			set
			{
				_parentFilename = value;
			}
		}

		[TypeConverter(typeof(Filename18StringConverter))]
		public Filename18 CurrentFilename
		{
			get
			{
				return _currentFilename;
			}
			set
			{
				_currentFilename = value;
			}
		}
		public string RootFilenameFragment
		{
			get
			{
				return _rootFilenameFragment;
			}
			set
			{
				_rootFilenameFragment = value;
			}
		}
		public UInt32 Timestamp
		{
			get
			{
				return _timeStamp;
			}
			set
			{
				_timeStamp = value;
			}
		}
		public bool LoopFlipnote
		{
			get
			{
				return _loop;
			}
			set
			{
				_loop = value;
			}
		}
		public int PlaybackSpeed
		{
			get
			{
				return _playbackSpeed;
			}
			set
			{
				if (value > 0 && value < 9)
				{
					_playbackSpeed = value;
				}
				else
				{
					throw new Exception("Invalid Playback Speed, Expected: 1-8, Received: " + value);
				}
			}
		}
#endregion
	}
}