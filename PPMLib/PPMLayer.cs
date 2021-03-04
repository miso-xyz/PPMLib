//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace PPMLib
{
	public class PPMLayer
	{
		private PenColor _pen;
		private bool _visibility;
		public bool[,] _layerData = new bool[193, 257];
		public byte[] _lineEncoding = new byte[49];
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property LinesEncoding(ByVal lineIndex As Integer) As LineEncoding
		public LineEncoding LinesEncoding(int lineIndex)
		{
			int _byte = _lineEncoding[lineIndex >> 2];
			int pos = (lineIndex & 0x3) * 2;
			return (LineEncoding)((_byte >> pos) & 0x3);
		}
			public void setLinesEncoding(int lineIndex, LineEncoding value)
			{
				int o = lineIndex >> 2;
				int pos = (lineIndex & 0x3) * 2;
				var b = _lineEncoding[o];
				b = (byte)(b & (byte)~(0x3 << pos));
				b = (byte)(b | (byte)((int)value << pos));
				_lineEncoding[o] = b;
			}
#region Line-Related Functions
		public LineEncoding SetLineEncodingForWholeLayer(int index)
		{
			var _0chks = 0;
			var _1chks = 0;
			for (var x = 0; x <= 32; x++)
			{
				var c = 8 * index;
				var n0 = 0;
				var n1 = 0;
				for (var x_ = 0; x_ <= 8; x_++)
				{
					if (Pixels(index, c + x_))
					{
						n1 += 1;
					}
					else
					{
						n0 += 1;
					}
				}
				_0chks += (n0 == 8) ? 1 : 0;
				_1chks += (n1 == 8) ? 1 : 0;
			}
//INSTANT C# NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
//			Select Case _0chks
//ORIGINAL LINE: Case 32
			if (_0chks == 32)
			{
					return LineEncoding.SkipLine;
			}
//ORIGINAL LINE: Case 0 AndAlso _1chks = 0
			else if (_0chks == ((_1chks == 0) ? -1 : 0))
			{
					return LineEncoding.RawLineData;
			}
//ORIGINAL LINE: Case Else
			else
			{
					return ((_0chks > _1chks) ? LineEncoding.CodedLine : LineEncoding.InvertedCodedLine);
			}
		}
		private void InsertLineInLayer(List<byte> lineData, int index, int layerIndex)
		{
			List<byte> chks = new List<byte>();
			switch (LinesEncoding(index))
			{
				case 0:
				{
					return;
				}
				case (LineEncoding)1:
				case (LineEncoding)2:
				{
					uint flag = 0;
					for (var x = 0; x <= 32; x++)
					{
						byte chunk = 0;
						for (var x_ = 0; x_ <= 8; x_++)
						{
							if (Pixels(index, 8 * x + x_))
							{
								chunk = (byte)(chunk | (byte)(1 << x_));
							}
						}
						if (chunk != ((LinesEncoding(index) == (PPMLib.LineEncoding)1) ? 0x0 : 0xFF))
						{
							flag |= (1U << (31 - x));
							chks.Add(chunk);
						}
					}
					lineData.Add((byte)((flag & 0xFF000000U) >> 24));
					lineData.Add((byte)((flag & 0xFF0000U) >> 16));
					lineData.Add((byte)((flag & 0xFF00U) >> 8));
					lineData.Add((byte)(flag & 0xFFU));
					lineData.AddRange(chks);
					return;
				}
				case (LineEncoding)3:
				{
					for (var x = 0; x <= 32; x++)
					{
						byte chunk = 0;
						for (var x_ = 0; x_ <= 8; x_++)
						{
							if (Pixels(index, 8 * x + x_))
							{
								chunk = (byte)(chunk | (byte)(1 << x_));
							}
						}
						chks.Add(chunk);
					}
					break;
				}
			}
		}
#endregion
		public bool Visible
		{
			get
			{
				return _visibility;
			}
			set
			{
				_visibility = value;
			}
		}
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property Pixels(ByVal x As Integer, ByVal y As Integer) As Boolean
		public bool Pixels(int x, int y)
		{
			return _layerData[y, x];
		}
			public void setPixels(int x, int y, bool value)
			{
				_layerData[y, x] = value;
			}
		public PenColor PenColor
		{
			get
			{
				return _pen;
			}
			set
			{
				_pen = value;
			}
		}
	}
}