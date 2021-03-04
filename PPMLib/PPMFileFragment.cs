using System;

namespace PPMLib
{
    public class PPMFileFragment
    {
        public PPMFileFragment(byte[] bytes)
        {
            Buffer = bytes;
        }

        private byte[] _Buffer;
        public byte[] Buffer
        {
            get => _Buffer;
            set
            {
                if (value.Length != 8)
                {
                    throw new ArgumentException("Wrong file fragment buffer size. It should be 8 bytes long");
                }
                _Buffer = value;
            }
        }
    }
}
