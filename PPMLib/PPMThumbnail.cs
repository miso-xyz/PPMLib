using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PPMLib
{
    public class PPMThumbnail
    {
        public PPMThumbnail(byte[] bytes)
        {
            Buffer = bytes;
        }

        private byte[] _Buffer;
        public byte[] Buffer
        {
            get => _Buffer;
            set
            {
                if(value.Length!=1536)
                {
                    throw new ArgumentException("Thumbnail buffer must be 1536 bytes long");
                }
                _Buffer = value;
            }
        }
    }
}
