using System.IO;
using System.Text;

namespace PPMLib.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadWChars(this BinaryReader br, int count)
            => Encoding.Unicode.GetString(br.ReadBytes(2 * count));

        public static PPMFilename ReadPPMFilename(this BinaryReader br)
            => new PPMFilename(br.ReadBytes(18));

        public static PPMFileFragment ReadPPMFileFragment(this BinaryReader br)
            => new PPMFileFragment(br.ReadBytes(8));

        public static PPMTimestamp ReadPPMTimestamp(this BinaryReader br)
            => new PPMTimestamp(br.ReadUInt32());

        public static PPMThumbnail ReadPPMThumbnail(this BinaryReader br)
            => new PPMThumbnail(br.ReadBytes(1536));

        public static PPMFrame ReadPPMFrame(this BinaryReader br)
        {
            PPMFrame frame = new PPMFrame();
            frame._firstByteHeader = br.ReadByte();
            if ((frame._firstByteHeader & 0x60) != 0)
            {
                frame._translateX = br.ReadSByte();
                frame._translateY = br.ReadSByte();
            }

            frame.PaperColor = (PaperColor)(frame._firstByteHeader % 2);
            frame.Layer1.PenColor = (PenColor)((frame._firstByteHeader >> 1) & 3);
            frame.Layer2.PenColor = (PenColor)((frame._firstByteHeader >> 3) & 3);

            frame.Layer1._linesEncoding = br.ReadBytes(0x30);
            frame.Layer2._linesEncoding = br.ReadBytes(0x30);

            for (int y = 0, yy; y < 192; y++)
            {
                yy = y << 5;
                switch (frame.Layer1.LinesEncoding(y))
                {
                    case 0:
                        break;
                    case (LineEncoding)1:
                        for (int x = 0; x < 32; x++) frame.Layer1._layerData[yy + x] = 0x00;
                        byte b1 = br.ReadByte(), b2 = br.ReadByte(), b3 = br.ReadByte(), b4 = br.ReadByte();
                        uint bytes = ((uint)(b1 << 24)) + ((uint)(b2 << 16)) + ((uint)(b3 << 8)) + b4;
                        while (bytes != 0)
                        {
                            if ((bytes & 0x80000000) != 0)
                                frame.Layer1._layerData[yy] = br.ReadByte();
                            bytes <<= 1;
                            yy++;
                        }
                        break;
                    case (LineEncoding)2:
                        for (int x = 0; x < 32; x++) frame.Layer1._layerData[yy + x] = 0xFF;
                        b1 = br.ReadByte(); b2 = br.ReadByte(); b3 = br.ReadByte(); b4 = br.ReadByte();
                        bytes = ((uint)(b1 << 24)) + ((uint)(b2 << 16)) + ((uint)(b3 << 8)) + b4;
                        while (bytes != 0)
                        {
                            if ((bytes & 0x80000000) != 0)
                                frame.Layer1._layerData[yy] = br.ReadByte();
                            bytes <<= 1;
                            yy++;
                        }
                        break;
                    case (LineEncoding)3:
                        for (int x = 0; x < 32; x++)
                            frame.Layer1._layerData[yy + x] = br.ReadByte();
                        break;
                }
            }
            for (int y = 0, yy; y < 192; y++)
            {
                yy = y << 5;
                switch (frame.Layer2.LinesEncoding(y))
                {
                    case 0:
                        break;
                    case (LineEncoding)1:
                        for (int x = 0; x < 32; x++) frame.Layer2._layerData[yy + x] = 0x00;
                        byte b1 = br.ReadByte(), b2 = br.ReadByte(), b3 = br.ReadByte(), b4 = br.ReadByte();
                        uint bytes = ((uint)(b1 << 24)) + ((uint)(b2 << 16)) + ((uint)(b3 << 8)) + b4;
                        while (bytes != 0)
                        {
                            if ((bytes & 0x80000000) != 0)
                                frame.Layer2._layerData[yy] = br.ReadByte();
                            bytes <<= 1;
                            yy++;
                        }
                        break;
                    case (LineEncoding)2:
                        for (int x = 0; x < 32; x++) frame.Layer2._layerData[yy + x] = 0xFF;
                        b1 = br.ReadByte(); b2 = br.ReadByte(); b3 = br.ReadByte(); b4 = br.ReadByte();
                        bytes = ((uint)(b1 << 24)) + ((uint)(b2 << 16)) + ((uint)(b3 << 8)) + b4;
                        while (bytes != 0)
                        {
                            if ((bytes & 0x80000000) != 0)
                                frame.Layer2._layerData[yy] = br.ReadByte();
                            bytes <<= 1;
                            yy++;
                        }
                        break;
                    case (LineEncoding)3:
                        for (int x = 0; x < 32; x++)
                            frame.Layer2._layerData[yy + x] = br.ReadByte();
                        break;
                }
            }
            return frame;
        }
    }
}
