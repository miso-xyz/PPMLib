using System;
using System.Collections.Generic;
using System.Diagnostics; using System.Drawing;  namespace PPMLib {     public class PPMFrame     {          private PPMLayer _layer1 = new PPMLayer();         private PPMLayer _layer2 = new PPMLayer();         private PaperColor _paperColor;         private Bitmap _frame = new Bitmap(256, 192);         private int _animationIndex;         public byte _firstByteHeader;         public int _translateX;         public int _translateY;          /// <summary>         /// Overwrite frame data         /// </summary>         /// <param name="prev">Frame data to apply</param>         public void Overwrite(PPMFrame prev) // Just use current frame         {             if ((_firstByteHeader & 0x80) != 0)             {                 return;             }                        // < There is NOT a mistake anywhere              int ld0 = ((_translateX >= 0) ? (_translateX >> 3) : 0);             int pi0 = (_translateX >= 0) ? 0 : ((-_translateX) >> 3);             byte del = (byte)(_translateX >= 0 ? (_translateX & 7) : (((byte)(_translateX)) & 7));                        byte ndel = (byte)(8 - del);             byte alpha = (byte)((1 << (8 - del)) - 1);             byte nalpha = (byte)~alpha;             int pi, ld;             if (_translateX >= 0)              {                 for (int y = 0; y < 192; y++)                 {                     if (y < _translateY) continue;                     if (y - _translateY >= 192) break;                     ld = (y << 5) + ld0;                     pi = ((y - _translateY) << 5) + pi0;                     Layer1[ld] ^= (byte)(prev.Layer1[pi] & alpha);                     Layer2[ld++] ^= (byte)(prev.Layer2[pi] & alpha);                     while ((ld & 31) < 31)                     {                         Layer1[ld] ^= (byte)(((prev.Layer1[pi] & nalpha) >> ndel) | ((prev.Layer1[pi + 1] & alpha) << del));                         Layer2[ld] ^= (byte)(((prev.Layer2[pi] & nalpha) >> ndel) | ((prev.Layer2[pi + 1] & alpha) << del));                                                 ld++; pi++;                     }                     Layer1[ld] ^= (byte)((prev.Layer1[pi] & nalpha) | (prev.Layer1[pi + 1] & alpha));                     Layer2[ld] ^= (byte)((prev.Layer2[pi] & nalpha) | (prev.Layer2[pi + 1] & alpha));                                     }             }             else             {                 for (ushort y = 0; y < 192; y++)                 {                     if (y < _translateY) continue;                     if (y - _translateY >= 192) break;                     ld = (y << 5) + ld0;                     pi = ((y - _translateY) << 5) + pi0;                     while ((pi & 31) < 31)                     {                         Layer1[ld] ^= (byte)(((prev.Layer1[pi] & nalpha) >> ndel) | ((prev.Layer1[pi + 1] & alpha) << del));                         Layer2[ld] ^= (byte)(((prev.Layer2[pi] & nalpha) >> ndel) | ((prev.Layer2[pi + 1] & alpha) << del));                         ld++; pi++;                     }                     Layer1[ld] ^= (byte)(prev.Layer1[pi] & nalpha);                     Layer2[ld] ^= (byte)(prev.Layer2[pi] & nalpha);                 }             }         }          public PPMFrame CreateDiff0(PPMFrame prev)
        {
            var frame = new PPMFrame();
            frame._firstByteHeader = _firstByteHeader;
            frame._firstByteHeader &= 0b00011111;
            frame._firstByteHeader |= 0x80;
            for (int i = 0; i < 256 * 192; i++)
            {
                frame.Layer1[i] = (byte)(Layer1[i] ^ prev.Layer1[i]);
                frame.Layer2[i] = (byte)(Layer2[i] ^ prev.Layer2[i]);
            }
            // > TO DO : update line encodings
            return frame;
        }          public PPMLayer Layer1         {             get => _layer1;         }         public PPMLayer Layer2         {             get => _layer2;                                }          public int AnimationIndex         {             get             {                 return _animationIndex;             }             set             {                 _animationIndex = value;             }         }          public PaperColor PaperColor         {             get             {                 return _paperColor;             }             set             {                 _paperColor = value;             }         }          public override string ToString()         {             return _firstByteHeader.ToString("X2");         }

        /// <summary>
        /// Converts frame data to flipnote PPM-format binary
        /// </summary>
        /// <returns>
        /// A byte array containing animation frame data as should
        /// be existent in a PPM file
        /// </returns>
        public byte[] ToByteArray()
        {
            var res = new List<byte>();
            res.Add(_firstByteHeader);
            res.AddRange(Layer1._linesEncoding);
            res.AddRange(Layer2._linesEncoding);
            for (int l = 0; l < 192; L1PutLine(res, l++)) ;
            for (int l = 0; l < 192; L2PutLine(res, l++)) ;
            return res.ToArray();
        }

        private void L1PutLine(List<byte> lst, int y)
        {
            int compr = (int)Layer1.LinesEncoding(y);
            if (compr == 0) return;
            if (compr == 1)
            {
                var chks = new List<byte>();
                uint flag = 0;
                for (int i = 0; i < 32; i++)
                {
                    byte chunk = 0;
                    for (int j = 0; j < 8; j++)
                        if (Layer1[8 * i + j, y]) 
                            chunk |= (byte)(1 << j);
                    if (chunk != 0x00)
                    {
                        flag |= (1u << (31 - i));
                        chks.Add(chunk);
                    }
                }
                lst.Add((byte)((flag & 0xFF000000u) >> 24));
                lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                lst.Add((byte)(flag & 0x000000FFu));
                lst.AddRange(chks);
                return;
            }
            if (compr == 2)
            {
                var chks = new List<byte>();
                uint flag = 0;
                for (int i = 0; i < 32; i++)
                {
                    byte chunk = 0;
                    for (int j = 0; j < 8; j++)
                        if (Layer1[8 * i + j, y]) 
                            chunk |= (byte)(1 << j);
                    if (chunk != 0xFF)
                    {
                        flag |= (1u << (31 - i));
                        chks.Add(chunk);
                    }
                }
                lst.Add((byte)((flag & 0xFF000000u) >> 24));
                lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                lst.Add((byte)(flag & 0x000000FFu));
                lst.AddRange(chks);
                return;
            }
            if (compr == 3)
            {
                for (int i = 0; i < 32; i++)
                {
                    byte chunk = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (Layer1[8 * i + j, y]) 
                            chunk |= (byte)(1 << j);
                    }
                    lst.Add(chunk);
                }
                return;
            }
        }

        private void L2PutLine(List<byte> lst, int y)
        {
            int compr = (int)Layer2.LinesEncoding(y);
            if (compr == 0) return;
            if (compr == 1)
            {
                var chks = new List<byte>();
                uint flag = 0;
                for (int i = 0; i < 32; i++)
                {
                    byte chunk = 0;
                    for (int j = 0; j < 8; j++)
                        if (Layer2[8 * i + j, y]) 
                            chunk |= (byte)(1 << j);
                    if (chunk != 0x00)
                    {
                        flag |= (1u << (31 - i));
                        chks.Add(chunk);
                    }
                }
                lst.Add((byte)((flag & 0xFF000000u) >> 24));
                lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                lst.Add((byte)(flag & 0x000000FFu));
                lst.AddRange(chks);
                return;
            }
            if (compr == 2)
            {
                var chks = new List<byte>();
                uint flag = 0;
                for (int i = 0; i < 32; i++)
                {
                    byte chunk = 0;
                    for (int j = 0; j < 8; j++)
                        if (Layer2[8 * i + j, y]) 
                            chunk |= (byte)(1 << j);
                    if (chunk != 0xFF)
                    {
                        flag |= (1u << (31 - i));
                        chks.Add(chunk);
                    }
                }
                lst.Add((byte)((flag & 0xFF000000u) >> 24));
                lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                lst.Add((byte)(flag & 0x000000FFu));
                lst.AddRange(chks);
                return;
            }
            if (compr == 3)
            {
                for (int i = 0; i < 32; i++)
                {
                    byte chunk = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (Layer2[8 * i + j, y]) 
                            chunk |= (byte)(1 << j);
                    }
                    lst.Add(chunk);
                }
                return;
            }
        }



    } }