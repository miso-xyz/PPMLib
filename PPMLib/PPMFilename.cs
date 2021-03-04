using System;
using System.Text.RegularExpressions;

namespace PPMLib
{
    public class PPMFilename
    {
        public PPMFilename(byte[] bytes)
        {
            Buffer = bytes;
        }

        public PPMFilename(string fn)
        {
            if (fn.Length != 24)
            {
                throw new ArgumentException("Wrong filename string length. It should be 24 characters long");
            }
            if (!Regex.IsMatch(fn, @"[0-9,A-F]{6}_[0-9,A-F]{13}_\d{3}"))
            {
                throw new FormatException("Incorrect filename");
            }
            Buffer = new byte[18];
            for (int i = 0; i < 3; i++)
                Buffer[i] = Convert.ToByte("" + fn[2 * i] + fn[2 * i + 1], 16);
            for (int i = 3, j = 7; i < 16; Buffer[i++] = (byte)fn[j++]) ;
            ushort b = Convert.ToUInt16(fn.Substring(21));
            Buffer[16] = (byte)b;
            b >>= 8;
            Buffer[17] = (byte)b;
        }

        private byte[] _Buffer;
        public byte[] Buffer
        {
            get => _Buffer;
            set
            {
                if (value.Length != 18)
                {
                    throw new ArgumentException("Wrong filename buffer size. It should be 18 bytes long");
                }
                _Buffer = value;
            }
        }

        public override string ToString()
        {
            var result = "";
            for (int i = 0; i < 3; result += Buffer[i++].ToString("X2")) ;
            result += "_";
            for (int i = 3; i < 16; result += Convert.ToChar(Buffer[i++])) ;
            return result + "_" + ((Buffer[17] << 4) | Buffer[16]).ToString("000");
        }
    }
}
