using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            
    }
}
