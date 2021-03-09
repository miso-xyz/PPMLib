using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPMLib.WPF
{
    public static class PPMRenderer
    {
        /// <summary>
        /// Color pallete used when rendering the thumbnail
        /// </summary>
        public static readonly List<Color> ThumbnailPalette = new List<Color>
        {
            Color.FromRgb(0xFF,0xFF,0xFF), Color.FromRgb(0x52,0x52,0x52), Color.FromRgb(0xFF,0xFF,0xFF), Color.FromRgb(0x9C,0x9C,0x9C),
            Color.FromRgb(0xFF,0x48,0x44), Color.FromRgb(0xC8,0x51,0x4F), Color.FromRgb(0xFF,0xAD,0xAC), Color.FromRgb(0x00,0xFF,0x00),
            Color.FromRgb(0x48,0x40,0xFF), Color.FromRgb(0x51,0x4F,0xB8), Color.FromRgb(0xAD,0xAB,0xFF), Color.FromRgb(0x00,0xFF,0x00),
            Color.FromRgb(0xB6,0x57,0xB7), Color.FromRgb(0x00,0xFF,0x00), Color.FromRgb(0x00,0xFF,0x00), Color.FromRgb(0x00,0xFF,0x00)
        };

        /// <summary>
        /// Converts the flipnote's thumbnail to Bitmap
        /// </summary>
        /// <param name="buffer">Raw Thumbnail Bytes</param>
        public static WriteableBitmap GetThumbnailBitmap(byte[] buffer)
        {
            if (buffer.Length != 1536)
            {
                throw new ArgumentException("Wrong thumbnail buffer size");
            }           
            
            byte[] bytes = new byte[32 * 48];            

            int offset = 0;
            for (int ty = 0; ty < 48; ty += 8)
            {
                for (int tx = 0; tx < 32; tx += 4)
                {
                    for (int l = 0; l < 8; l++)
                    {
                        int line = (ty + l) << 5;
                        for (int px = 0; px < 4; px++)
                        {
                            // Need to reverse nibbles :
                            bytes[line + tx + px] = (byte)(((buffer[offset] & 0xF) << 4) | ((buffer[offset] & 0xF0) >> 4));
                            offset += 1;
                        }
                    }
                }
            }

            var palette = new BitmapPalette(ThumbnailPalette);
            // Directly set bitmap's 4-bit palette instead of using 32-bit colors 
            var bmp = new WriteableBitmap(64, 48, 96, 96, PixelFormats.Indexed4, palette);
            bmp.WritePixels(new System.Windows.Int32Rect(0, 0, 64, 48), bytes, 32, 0);
            return bmp;
        }

        /// <summary>
        /// All colors available for frames
        /// </summary>
        public static readonly List<Color> FramePalette = new List<Color>
        {
            Color.FromRgb(0x0e,0x0e,0x0e), Color.FromRgb(0xFF,0xFF,0xFF), Color.FromRgb(0xFF,0x00,0x00), Color.FromRgb(0x00,0x00,0xFF) 
        };

        /// <summary>
        /// Get the pen color of the choosen layer
        /// </summary>
        /// <param name="pc">Pen Color (of the layer)</param>
        /// <param name="paper">Paper Color (of the frame)</param>
        /// <returns>Color</returns>
        private static Color GetLayerColor(PenColor pc,PaperColor paper)
        {
            if (pc == PenColor.Inverted) return FramePalette[1 - (int)paper];
            return FramePalette[(int)pc];
        }

        /// <summary>
        /// Renders the given frame to a WritableBitmap
        /// </summary>
        /// <param name="frame">Frame Data</param>
        /// <returns>Rendered Frame</returns>
        public static WriteableBitmap GetFrameBitmap(PPMFrame frame)
        {
            var palette = new BitmapPalette(new List<Color>
            {
                FramePalette[(int)frame.PaperColor],
                GetLayerColor(frame.Layer1.PenColor,frame.PaperColor),
                GetLayerColor(frame.Layer2.PenColor,frame.PaperColor),                
            });
            var bmp = new WriteableBitmap(256, 192, 96, 96, PixelFormats.Indexed2, palette);

            int stride = 64;
            byte[] pixels = new byte[64 * 192];
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 192; y++)
                {
                    if (frame.Layer1[y, x]) 
                    {
                        int b = 256 * y + x;
                        int p = 3 - b % 4;
                        b /= 4;
                        pixels[b] &= (byte)(~(0b11 << (2 * p)));
                        pixels[b] |= (byte)(0b10 << (2 * p));
                    }
                }
            }
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 192; y++)
                {
                    if (frame.Layer2[y, x])
                    {
                        int b = 256 * y + x;
                        int p = 3 - b % 4;
                        b /= 4;
                        pixels[b] &= (byte)(~(0b11 << (2 * p)));
                        pixels[b] |= (byte)(0b01 << (2 * p));
                    }
                }
            }
            bmp.WritePixels(new System.Windows.Int32Rect(0, 0, 256, 192), pixels, stride, 0);
            return bmp;
        }

    }
}
