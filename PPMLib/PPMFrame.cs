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
            Debug.WriteLine("Yeah");
            for (var y = 0; y <= 191; y++)
            {
                if (y - _translateY < 0)
                {
                    continue;
                }
                else if (y - _translateY >= 192)
                {
                    break;
                }
                for (var x = 0; x <= 255; x++)
                {
                    if (x - _translateX < 0)
                    {
                        continue;
                    }
                    else if (x - _translateX >= 256)
                    {
                        break;
                    }
                    Layer1[y, x] = Layer1[y, x] ^ frame.Layer1[y - _translateY, x - _translateX];
                    Layer2[y, x] = Layer2[y, x] ^ frame.Layer2[y - _translateY, x - _translateX];
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
    }
}