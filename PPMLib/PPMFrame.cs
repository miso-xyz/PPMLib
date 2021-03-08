//INSTANT C# NOTE: Formerly VB project-level imports:
using System.Diagnostics;
using System.Drawing;

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
            int tX = frame._translateX;
            int tY = frame._translateY;
            //////////// < There is a mistake somewhere 
            int ld0 = ((tX >= 0) ? (tX >> 3) : 0);
            int pi0 = (tX >= 0) ? 0 : ((-tX) >> 3);
            byte del = (byte)(tX >= 0 ? (tX & 7) : (((byte)(tX)) & 7));
            if (del != 0) Debug.WriteLine(tX + " " + del);
            byte ndel = (byte)(8 - del);
            byte alpha = (byte)((1 << (8 - del)) - 1);
            byte nalpha = (byte)~alpha;
            int pi = 0, ld = 0;
            if (tX >= 0)
            {
                for (ushort y = 0; y < 192; y++)
                {
                    if (y < tY) continue;
                    if (y - tY >= 192) break;
                    ld = ((y << 5) + ld0);
                    pi = (((y - tY) << 5) + pi0);
                    Layer1._layerData[ld] ^= (byte)(frame.Layer1._layerData[pi] & alpha);
                    Layer2._layerData[ld++] ^= (byte)(frame.Layer2._layerData[pi] & alpha);
                    while ((ld & 31) < 31)
                    {
                        Layer1._layerData[ld] ^= (byte)(((frame.Layer1._layerData[pi] & nalpha) >> ndel) | ((frame.Layer1._layerData[pi + 1] & alpha) << del));
                        Layer2._layerData[ld] ^= (byte)(((frame.Layer2._layerData[pi] & nalpha) >> ndel) | ((frame.Layer2._layerData[pi + 1] & alpha) << del));
                        //layer2[ld] ^= ((layerB[pi] & nalpha) >> ndel) | ((layerB[pi + 1] & alpha) << del);
                        ld++; pi++;
                    }
                    Layer1._layerData[ld] ^= (byte)((frame.Layer1._layerData[pi] & nalpha) | (frame.Layer1._layerData[pi + 1] & alpha));
                    Layer2._layerData[ld] ^= (byte)((frame.Layer2._layerData[pi] & nalpha) | (frame.Layer2._layerData[pi + 1] & alpha));
                    //layer2[ld] ^= (layerB[pi] & nalpha) | (layerB[pi + 1] & alpha);
                }
            }
            else
            {
                for (ushort y = 0; y < 192; y++)
                {
                    if (y < tY) continue;
                    if (y - tY >= 192) break;
                    ld = ((y << 5) + ld0);
                    pi = (((y - tY) << 5) + pi0);
                    while ((pi & 31) < 31)
                    {
                        Layer1._layerData[ld] ^= (byte)(((frame.Layer1._layerData[pi] & nalpha) >> ndel) | ((frame.Layer1._layerData[pi + 1] & alpha) << del));
                        Layer2._layerData[ld] ^= (byte)(((frame.Layer2._layerData[pi] & nalpha) >> ndel) | ((frame.Layer2._layerData[pi + 1] & alpha) << del));
                        ld++; pi++;
                    }
                    Layer1._layerData[ld] ^= (byte)(frame.Layer1._layerData[pi] & nalpha);
                    Layer2._layerData[ld] ^= (byte)(frame.Layer2._layerData[pi] & nalpha);
                    //layer2[ld] ^= layerB[pi] & nalpha;
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