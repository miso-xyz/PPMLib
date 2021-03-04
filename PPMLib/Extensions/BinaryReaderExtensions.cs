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
            frame.Layer1.PenColor = (PenColor)((frame._firstByteHeader & 0x6) >> 1);
            frame.Layer2.PenColor = (PenColor)((frame._firstByteHeader & 0x18) >> 3);

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

        private static void PPMLineEncDealWith4Bytes(BinaryReader r, PPMFrame fd, int layer, int line, bool inv = false)
        {
            int y = 0;
            if (inv)
            {
                for (int i = 0; i < 256; i++)
                    if (layer == 1)
                        fd.Layer1[line, i] = true;
                    else
                        fd.Layer2[line, i] = true;
            }
            byte b1 = r.ReadByte(),
                b2 = r.ReadByte(),
                b3 = r.ReadByte(),
                b4 = r.ReadByte();

            uint bytes = ((uint)(b1 << 24)) + ((uint)(b2 << 16)) + ((uint)(b3 << 8)) + b4;
            while (bytes != 0)
            {
                if ((bytes & 0x80000000) != 0)
                {
                    var pixels = r.ReadByte();
                    for (int i = 0; i < 8; i++)
                    {
                        if (layer == 1)
                            fd.Layer1[line, y++] = ((pixels >> i) & 1) == 1;
                        else
                            fd.Layer2[line, y++] = ((pixels >> i) & 1) == 1;
                    }
                }
                else y += 8;
                bytes <<= 1;
            }
        }

        private static void PPMLineEncDealWithRawData(BinaryReader r, PPMFrame fd, int layer, int line)
        {
            int y = 0;
            for (int i = 0; i < 32; i++)
            {
                byte val = r.ReadByte();
                for (int b = 0; b < 8; b++)
                    if (layer == 1)
                        fd.Layer1[line, y++] = ((val >> b) & 1) == 1;
                    else
                        fd.Layer2[line, y++] = ((val >> b) & 1) == 1;
            }
        }

    }
}
