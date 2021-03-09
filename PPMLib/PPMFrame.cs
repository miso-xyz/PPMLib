//INSTANT C# NOTE: Formerly VB project-level imports:
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PPMLib
{
    public class PPMFrame
    {

        private PPMLayer _layer1 = new PPMLayer();
        private PPMLayer _layer2 = new PPMLayer();
        private PaperColor _paperColor;
        private Bitmap _frame = new Bitmap(256, 192);
        private int _animationIndex;
        public byte _firstByteHeader;
        public int _translateX;
        public int _translateY;
        public void Overwrite(PPMFrame frame)
        {
            if ((_firstByteHeader & 0x80) != 0)
            {
                return;
            }           
            // < There is NOT a mistake anywhere 
            int ld0 = ((_translateX >= 0) ? (_translateX >> 3) : 0);
            int pi0 = (_translateX >= 0) ? 0 : ((-_translateX) >> 3);
            byte del = (byte)(_translateX >= 0 ? (_translateX & 7) : (((byte)(_translateX)) & 7));           
            byte ndel = (byte)(8 - del);
            byte alpha = (byte)((1 << (8 - del)) - 1);
            byte nalpha = (byte)~alpha;
            int pi, ld;
            if (_translateX >= 0) 
            {
                for (int y = 0; y < 192; y++)
                {
                    if (y < _translateY) continue;
                    if (y - _translateY >= 192) break;
                    ld = (y << 5) + ld0;
                    pi = ((y - _translateY) << 5) + pi0;
                    Layer1[ld] ^= (byte)(frame.Layer1[pi] & alpha);
                    Layer2[ld++] ^= (byte)(frame.Layer2[pi] & alpha);
                    while ((ld & 31) < 31)
                    {
                        Layer1[ld] ^= (byte)(((frame.Layer1[pi] & nalpha) >> ndel) | ((frame.Layer1[pi + 1] & alpha) << del));
                        Layer2[ld] ^= (byte)(((frame.Layer2[pi] & nalpha) >> ndel) | ((frame.Layer2[pi + 1] & alpha) << del));                        
                        ld++; pi++;
                    }
                    Layer1[ld] ^= (byte)((frame.Layer1[pi] & nalpha) | (frame.Layer1[pi + 1] & alpha));
                    Layer2[ld] ^= (byte)((frame.Layer2[pi] & nalpha) | (frame.Layer2[pi + 1] & alpha));                    
                }
            }
            else
            {
                for (ushort y = 0; y < 192; y++)
                {
                    if (y < _translateY) continue;
                    if (y - _translateY >= 192) break;
                    ld = (y << 5) + ld0;
                    pi = ((y - _translateY) << 5) + pi0;
                    while ((pi & 31) < 31)
                    {
                        Layer1[ld] ^= (byte)(((frame.Layer1[pi] & nalpha) >> ndel) | ((frame.Layer1[pi + 1] & alpha) << del));
                        Layer2[ld] ^= (byte)(((frame.Layer2[pi] & nalpha) >> ndel) | ((frame.Layer2[pi + 1] & alpha) << del));
                        ld++; pi++;
                    }
                    Layer1[ld] ^= (byte)(frame.Layer1[pi] & nalpha);
                    Layer2[ld] ^= (byte)(frame.Layer2[pi] & nalpha);
                }
            }
        }       

        public PPMLayer Layer1
        {
            get
            {
                return _layer1;
            }
        }
        public PPMLayer Layer2
        {
            get
            {
                return _layer2;
            }
        }

        public int AnimationIndex
        {
            get
            {
                return _animationIndex;
            }
            set
            {
                _animationIndex = value;
            }
        }

        public PaperColor PaperColor
        {
            get
            {
                return _paperColor;
            }
            set
            {
                _paperColor = value;
            }
        }

        public override string ToString()
        {
            return _firstByteHeader.ToString("X2");
        }
    }
}