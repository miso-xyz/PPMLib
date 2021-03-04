using System;
using System.Runtime.InteropServices;

namespace PPMLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WavePcmFormat
    {
        /* ChunkID          Contains the letters "RIFF" in ASCII form */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] chunkID = new char[] { 'R', 'I', 'F', 'F' };

        /* ChunkSize        36 + SubChunk2Size */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint chunkSize = 0;

        /* Format           The "WAVE" format name */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] format = new char[] { 'W', 'A', 'V', 'E' };

        /* Subchunk1ID      Contains the letters "fmt " */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] subchunk1ID = new char[] { 'f', 'm', 't', ' ' };

        /* Subchunk1Size    16 for PCM */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint subchunk1Size = 16;

        /* AudioFormat      PCM = 1 (i.e. Linear quantization) */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort audioFormat = 1;

        /* NumChannels      Mono = 1, Stereo = 2, etc. */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort numChannels = 1;
        public ushort NumChannels { get => numChannels; set => numChannels = value; }

        /* SampleRate       8000, 44100, etc. */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint sampleRate = 44100;
        public uint SampleRate { get => sampleRate; set => sampleRate = value; }

        /* ByteRate         == SampleRate * NumChannels * BitsPerSample/8 */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint byteRate = 0;

        /* BlockAlign       == NumChannels * BitsPerSample/8 */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort blockAlign = 0;

        /* BitsPerSample    8 bits = 8, 16 bits = 16, etc. */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort bitsPerSample = 8;
        public ushort BitsPerSample { get => bitsPerSample; set => bitsPerSample = value; }

        /* Subchunk2ID      Contains the letters "data" */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] subchunk2ID = new char[] { 'd', 'a', 't', 'a' };

        /* Subchunk2Size    == NumSamples * NumChannels * BitsPerSample/8 */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint subchunk2Size = 0;

        /* Data             The actual sound data. */
        public byte[] Data { get; set; } = new byte[0];

        public WavePcmFormat(byte[] data, ushort numChannels = 1, uint sampleRate = 8192, ushort bitsPerSample = 16)
        {
            Data = data;
            NumChannels = numChannels;
            SampleRate = sampleRate;
            BitsPerSample = bitsPerSample;
        }

        private void CalculateSizes()
        {
            subchunk2Size = (uint)Data.Length;
            blockAlign = (ushort)(NumChannels * BitsPerSample / 8);
            byteRate = SampleRate * NumChannels * BitsPerSample / 8;
            chunkSize = 36 + subchunk2Size;
        }

        public byte[] ToBytesArray()
        {
            CalculateSizes();
            int headerSize = Marshal.SizeOf(this);
            IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
            Marshal.StructureToPtr(this, headerPtr, false);
            byte[] rawData = new byte[headerSize + Data.Length];
            Marshal.Copy(headerPtr, rawData, 0, headerSize);
            Marshal.FreeHGlobal(headerPtr);
            Array.Copy(Data, 0, rawData, 44, Data.Length);
            return rawData;
        }
    }
}
