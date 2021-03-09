//INSTANT C# NOTE: Formerly VB project-level imports:
using System.Collections.Generic;

namespace PPMLib
{
    public class PPMLayer
    {
        private PenColor _pen;
        private bool _visibility;
        internal byte[] _layerData = new byte[32 * 192];        
        internal byte[] _linesEncoding = new byte[48];
        public LineEncoding LinesEncoding(int lineIndex)
            => (LineEncoding)((_linesEncoding[lineIndex >> 2] >> ((lineIndex & 0x3) << 1)) & 0x3);

        public void setLinesEncoding(int lineIndex, LineEncoding value)
        {
            int o = lineIndex >> 2;
            int pos = (lineIndex & 0x3) * 2;
            var b = _linesEncoding[o];
            b = (byte)(b & (byte)~(0x3 << pos));
            b = (byte)(b | (byte)((int)value << pos));
            _linesEncoding[o] = b;
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
                    if (this[index, c + x_])
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
            if (_0chks == 32)
            {
                return LineEncoding.SkipLine;
            }
            else if (_0chks == ((_1chks == 0) ? -1 : 0))
            {
                return LineEncoding.RawLineData;
            }
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
                                if (this[index, 8 * x + x_])
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
                                if (this[index, 8 * x + x_])
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

        public byte this[int p]
        {
            get => _layerData[p];
            set => _layerData[p] = value;
        }

        public bool this[int y, int x]
        {
            get
            {
                int p = 256 * y + x;
                return (_layerData[p >> 3] & ((byte)(1 << (p & 7)))) != 0;
            }
            set
            {
                int p = 256 * y + x;
                _layerData[p >> 3] &= (byte)(~(1 << (p & 0x7)));
                _layerData[p >> 3] |= (byte)((value ? 1 : 0) << (p & 0x7));
            }
        }
        public PenColor PenColor { get; set; }
    }
}