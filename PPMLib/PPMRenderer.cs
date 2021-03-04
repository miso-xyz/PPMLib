//INSTANT C# NOTE: Formerly VB project-level imports:
using System;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PPMLib
{
    public static class PPMRenderer
    {
        public static readonly Color[] ThumbnailPalette = { Color.FromArgb(unchecked((int)0xFFFFFFFF)), Color.FromArgb(unchecked((int)0xFF525252)), Color.FromArgb(unchecked((int)0xFFFFFFFF)), Color.FromArgb(unchecked((int)0xFF9C9C9C)), Color.FromArgb(unchecked((int)0xFFFF4844)), Color.FromArgb(unchecked((int)0xFFC8514F)), Color.FromArgb(unchecked((int)0xFFFFADAC)), Color.FromArgb(unchecked((int)0xFF00FF00)), Color.FromArgb(unchecked((int)0xFF4840FF)), Color.FromArgb(unchecked((int)0xFF514FB8)), Color.FromArgb(unchecked((int)0xFFADABFF)), Color.FromArgb(unchecked((int)0xFF00FF00)), Color.FromArgb(unchecked((int)0xFFB657B7)), Color.FromArgb(unchecked((int)0xFF00FF00)), Color.FromArgb(unchecked((int)0xFF00FF00)), Color.FromArgb(unchecked((int)0xFF00FF00)) };
        public static Bitmap GetThumbnailBitmap(byte[] buffer)
        {
            if (buffer.Length != 1536)
            {
                throw new ArgumentException("Wrong thumbnail buffer size");
            }
            // Directly set bitmap's 4-bit palette instead of using 32-bit colors 
            var bmp = new Bitmap(64, 48, PixelFormat.Format4bppIndexed);
            var palette = bmp.Palette;
            var entries = palette.Entries;
            for (var i = 0; i <= 15; i++)
            {
                entries[i] = ThumbnailPalette[i];
            }
            bmp.Palette = palette;

            var rect = new Rectangle(0, 0, 64, 48);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            byte[] bytes = new byte[(32 * 48) + 1];
            var IPtr = bmpData.Scan0;
            Marshal.Copy(IPtr, bytes, 0, 32 * 48);

            int offset = 0;
            for (int ty = 0; ty <= 47; ty += 8)
            {
                for (int tx = 0; tx <= 31; tx += 4)
                {
                    for (int l = 0; l <= 7; l++)
                    {
                        int line = (ty + l) << 5;
                        for (int px = 0; px <= 3; px++)
                        {
                            // Need to reverse nibbles :
                            bytes[line + tx + px] = (byte)(((buffer[offset] & 0xF) << 4) | ((buffer[offset] & 0xF0) >> 4));
                            offset += 1;
                        }
                    }
                }
            }

            Marshal.Copy(bytes, 0, IPtr, 32 * 48);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public static readonly Color[] FramePalette = { Color.FromArgb(unchecked((int)0xFF000000)), Color.FromArgb(unchecked((int)0xFFFFFFFF)), Color.FromArgb(unchecked((int)0xFFFF0000)), Color.FromArgb(unchecked((int)0xFF0000FF)) };

        public static Bitmap GetFrameBitmap(PPMFrame frame)
        {
            var bmp = new Bitmap(256, 192, PixelFormat.Format8bppIndexed);
            var palette = bmp.Palette;
            var entries = palette.Entries;
            for (var i = 0; i <= 3; i++)
            {
                entries[i] = FramePalette[i];
            }
            bmp.Palette = palette;

            var rect = new Rectangle(0, 0, 256, 192);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            byte[] bytes = new byte[(256 * 192) + 1];
            var IPtr = bmpData.Scan0;
            Marshal.Copy(IPtr, bytes, 0, 256 * 192);
            for (var y = 0; y <= 191; y++)
            {
                for (var x = 0; x <= 255; x++)
                {
                    if (frame.Layer2[y, x])
                    {
                        if (frame.Layer2.PenColor != PenColor.Inverted)
                        {
                            bytes[256 * y + x] = (byte)frame.Layer2.PenColor;
                        }
                        else
                        {
                            bytes[256 * y + x] = (byte)(1 - (int)frame.PaperColor);
                        }
                    }
                    else
                    {
                        if (frame.Layer1[y, x])
                        {
                            if (frame.Layer1.PenColor != PenColor.Inverted)
                            {
                                bytes[256 * y + x] = (byte)frame.Layer1.PenColor;
                            }
                            else
                            {
                                bytes[256 * y + x] = (byte)(1 - (int)frame.PaperColor);
                            }
                        }
                        else
                        {
                            bytes[256 * y + x] = (byte)frame.PaperColor;
                        }
                    }
                }
            }
            Marshal.Copy(bytes, 0, IPtr, 256 * 192);
            bmp.UnlockBits(bmpData);
            return bmp;
        }
    }

}