using PPMLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace PPMLib
{
    public class PPMFile
    {

        public bool Signed { get; set; }

        /// <summary>
        /// Read file as a flipnote
        /// </summary>
        /// <param name="path">Path to Flipnote</param>
        public void LoadFrom(string path) //private static
        {
            Parse(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Parse a flipnote's raw bytes
        /// </summary>
        /// <param name="bytes">Raw Flipnote Bytes</param>
        public void Parse(byte[] bytes)
        {
            var br = new BinaryReader(new MemoryStream(bytes));
            if (!br.ReadChars(4).SequenceEqual(FileMagic))
            {
                throw new FileFormatException("Unexpected file format");
            }
            AnimationDataSize = br.ReadUInt32();
            SoundDataSize = br.ReadUInt32();
            FrameCount = (ushort)(br.ReadUInt16() + 1);
            FormatVersion = br.ReadUInt16();
            IsLocked = br.ReadUInt16() != 0;
            ThumbnailFrameIndex = br.ReadUInt16();
            string rootname = br.ReadWChars(11);
            string parentname = br.ReadWChars(11);
            string currentname = br.ReadWChars(11);
            ulong parentid = br.ReadUInt64();
            ulong currentid = br.ReadUInt64();
            ParentFilename = br.ReadPPMFilename();
            CurrentFilename = br.ReadPPMFilename();
            ulong rootid = br.ReadUInt64();
            RootAuthor = new PPMAuthor(rootname, rootid);
            ParentAuthor = new PPMAuthor(parentname, parentid);
            CurrentAuthor = new PPMAuthor(currentname, currentid);
            RootFileFragment = br.ReadPPMFileFragment();
            Timestamp = br.ReadPPMTimestamp();
            br.ReadUInt16(); // 0x9E
            Thumbnail = br.ReadPPMThumbnail();
            FrameOffsetTableSize = br.ReadUInt16();
            br.ReadUInt32(); //0x6A2 - always 0
            AnimationFlags = br.ReadUInt16();
            var oCnt = FrameOffsetTableSize / 4.0 - 1;
            _animationOffset = new uint[(int)oCnt + 1];
            Frames = new PPMLib.PPMFrame[FrameCount];
            for (var x = 0; x <= oCnt; x++)
            {
                _animationOffset[x] = br.ReadUInt32();
            }
            long framePos0 = br.BaseStream.Position;
            var offset = framePos0; //&H6A8 + FrameOffsetTableSize
            for (var x = 0; x <= oCnt; x++)
            {
                if (offset + _animationOffset[x] == 4288480943)
                {
                    throw new Exception("Data corrupted (possible memory pit?)");
                }
                br.BaseStream.Seek(offset + _animationOffset[x], SeekOrigin.Begin);
                Frames[x] = br.ReadPPMFrame();
                Frames[x].AnimationIndex = Array.IndexOf(_animationOffset, _animationOffset[x]);
                if (x > 0)
                {
                    Frames[x].Overwrite(Frames[x - 1]);
                }
            }

            // Read Sound Data
            if (SoundDataSize == 0) return;

            offset = 0x6A0 + AnimationDataSize;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            SoundEffectFlags = new byte[Frames.Length];
            Audio = new PPMAudio();
            for (int i = 0; i < Frames.Length; i++)
            {
                SoundEffectFlags[i] = br.ReadByte();
            }
            offset += Frames.Length;

            // make the next offset dividable by 4
            br.ReadBytes((int)((4 - offset % 4) % 4));

            Audio.SoundData = new _SoundData();
            Audio.SoundHeader = new _SoundHeader();

            Audio.SoundHeader.BGMTrackSize = br.ReadUInt32();
            Audio.SoundHeader.SE1TrackSize = br.ReadUInt32();
            Audio.SoundHeader.SE2TrackSize = br.ReadUInt32();
            Audio.SoundHeader.SE3TrackSize = br.ReadUInt32();
            Audio.SoundHeader.CurrentFramespeed = (byte)(8 - br.ReadByte());
            Audio.SoundHeader.RecordingBGMFramespeed = (byte)(8 - br.ReadByte());

            // 
            Framerate = PPM_FRAMERATES[Audio.SoundHeader.CurrentFramespeed];
            BGMRate = PPM_FRAMERATES[Audio.SoundHeader.RecordingBGMFramespeed];
            br.ReadBytes(14);

            Audio.SoundData.RawBGM = br.ReadBytes((int)Audio.SoundHeader.BGMTrackSize);
            Audio.SoundData.RawSE1 = br.ReadBytes((int)Audio.SoundHeader.SE1TrackSize);
            Audio.SoundData.RawSE2 = br.ReadBytes((int)Audio.SoundHeader.SE2TrackSize);
            Audio.SoundData.RawSE3 = br.ReadBytes((int)Audio.SoundHeader.SE3TrackSize);

            // Read Signature (Will implement later)
            if (br.BaseStream.Position == br.BaseStream.Length)
            {
                // file is RSA unsigned -> do something...
            }
            else
            {
                // Next 0x80 bytes = RSA-1024 SHA-1 signature
                Signature = br.ReadBytes(0x80);
                var padding = br.ReadBytes(0x10);
                // Next 0x10 bytes are filled with 0
            }

        }

        public static PPMFile Create(PPMAuthor author, List<PPMFrame> frames, byte[] audio, bool ignoreMetadata = false)
        {
            var file = new PPMFile();
            file.FrameCount = (ushort)(frames.Count - 1);
            file.FormatVersion = 0x24;
            
            if (!ignoreMetadata)
            {
                file.RootAuthor = author;
                file.ParentAuthor = author;
                file.CurrentAuthor = author;

                string mac6 = string.Join("", BitConverter.GetBytes(author.Id).Take(3).Reverse().Select(t => t.ToString("X2")));
                var asm = Assembly.GetEntryAssembly().GetName().Version;
                var dt = DateTime.UtcNow;
                var fnVM = ((byte)asm.Major).ToString("X2");
                var fnVm = ((byte)asm.Minor).ToString("X2");
                var fnYY = (byte)(dt.Year - 2009);
                var fnMD = dt.Month * 32 + dt.Day;
                var fnTi = (((dt.Hour * 3600 + dt.Minute * 60 + dt.Second) % 4096) >> 1) + (fnMD > 255 ? 1 : 0);
                fnMD = (byte)fnMD;
                var fnYMD = (fnYY << 9) + fnMD;
                var H6_9 = fnYMD.ToString("X4");
                var H89 = ((byte)fnMD).ToString("X2");
                var HABC = fnTi.ToString("X3");

                string _13str = $"80{fnVM}{fnVm}{H6_9}{HABC}";
                string nEdited = 0.ToString().PadLeft(3, '0');
                var filename = $"{mac6}_{_13str}_{nEdited}.ppm";


                var rawfn = new byte[18];
                for (int i = 0; i < 3; i++)
                {
                    rawfn[i] = byte.Parse("" + mac6[2 * i] + mac6[2 * i + 1], System.Globalization.NumberStyles.HexNumber);
                }
                for (int i = 3; i < 16; i++)
                {
                    rawfn[i] = (byte)_13str[i - 3];
                }
                rawfn[16] = rawfn[17] = 0;

                file.ParentFilename = new PPMFilename(rawfn);
                file.CurrentFilename = new PPMFilename(rawfn);

                var ByteRootFileFragment = new byte[8];
                for (int i = 0; i < 3; i++)
                {
                    ByteRootFileFragment[i] =
                        byte.Parse("" + mac6[2 * i] + mac6[2 * i + 1], System.Globalization.NumberStyles.HexNumber);
                }
                for (int i = 3; i < 8; i++)
                {
                    ByteRootFileFragment[i] =
                        (byte)((byte.Parse("" + _13str[2 * (i - 3)], System.Globalization.NumberStyles.HexNumber) << 4)
                              + byte.Parse("" + _13str[2 * (i - 3) + 1], System.Globalization.NumberStyles.HexNumber));
                }

                file.RootFileFragment = new PPMFileFragment(ByteRootFileFragment);

                file.Timestamp = new PPMTimestamp((uint)((dt - new DateTime(2000, 1, 1, 0, 0, 0)).TotalSeconds));
                file.Thumbnail = new PPMThumbnail(new byte[0x600]);
                file.ThumbnailFrameIndex = 0;
            }
            // write the audio data

            uint animDataSize = (uint)(8 + 4 * frames.Count);

            file.AnimationFlags = 0x43;
            file.FrameOffsetTableSize = (ushort)(4 * frames.Count);            

            file.Frames = new PPMFrame[frames.Count];
            //SoundEffectFlags = new byte[frames.Count];

            for (int i = 0; i < frames.Count; i++)
            {

                file.Frames[i] = frames[i];


                animDataSize += (uint)file.Frames[i].ToByteArray().Length;
            }
            while ((animDataSize & 0x3) != 0) animDataSize++;
            file.AnimationDataSize = animDataSize;

            file.Audio = new PPMAudio();
            file.Audio.SoundData.RawBGM = audio;
            file.Audio.SoundHeader.BGMTrackSize = (uint)file.Audio.SoundData.RawBGM.Length;
            file.Audio.SoundHeader.SE1TrackSize = 0;
            file.Audio.SoundHeader.SE2TrackSize = 0;
            file.Audio.SoundHeader.SE2TrackSize = 0;
            file.SoundDataSize = (uint)file.Audio.SoundData.RawBGM.Length;

            file.Audio.SoundHeader.CurrentFramespeed = 0;
            file.Audio.SoundHeader.RecordingBGMFramespeed = 0;

            return file;
        }

        public void Save(string fn)
        {
            using (var w = new BinaryWriter(new FileStream(fn, FileMode.Create)))
            {
                //AnimationDataSize = (uint)(AnimationDataSize + 8 + Frames.Count() * 4);
                //var AllignSize = (uint)(4 - ((0x6A0 + AnimationDataSize + Frames.Count()) % 4));
                //if (AllignSize != 4)
                //    AnimationDataSize += AllignSize;
                w.Write(FileMagic);
                w.Write(AnimationDataSize);
                w.Write(SoundDataSize);
                w.Write(FrameCount);
                w.Write((ushort)0x0024);
                w.Write((ushort)(IsLocked ? 1 : 0));
                w.Write(ThumbnailFrameIndex);
                w.Write(Encoding.Unicode.GetBytes(RootAuthor._Name.PadRight(11, '\0')));
                w.Write(Encoding.Unicode.GetBytes(ParentAuthor._Name.PadRight(11, '\0')));
                w.Write(Encoding.Unicode.GetBytes(CurrentAuthor._Name.PadRight(11, '\0')));
                w.Write(ParentAuthor.Id);
                w.Write(CurrentAuthor.Id);
                w.Write(ParentFilename.Buffer);
                w.Write(CurrentFilename.Buffer);
                w.Write(RootAuthor.Id);
                w.Write(RootFileFragment.Buffer);
                w.Write(Timestamp.Value);
                w.Write((ushort)0); //0x009E
                w.Write(Thumbnail.Buffer);

                w.Write(FrameOffsetTableSize);
                w.Write((uint)0); // 0x06A2
                System.Diagnostics.Debug.WriteLine(AnimationFlags);
                w.Write(AnimationFlags);

                // Calculate frame offsets & write frame data
                List<byte[]> lst = new List<byte[]>();
                uint offset = 0;
                for (int i = 0; i < Frames.Length; i++)
                {
                    lst.Add(Frames[i].ToByteArray());
                    //MessageBox.Show(offset.ToString());
                    w.Write(offset);
                    offset += (uint)lst[i].Length;
                }

                for (int i = 0; i < Frames.Length; i++)
                {
                    w.Write(lst[i]);
                }

                w.Write(new byte[(4 - w.BaseStream.Position % 4) % 4]);

                // Write sound data
                for (int i = 0; i < Frames.Length; i++)
                    w.Write((byte)0);

                //if (AllignSize != 4)
                //    w.Write(new byte[AllignSize]);
                w.Write(new byte[(4 - w.BaseStream.Position % 4) % 4]);
                // make the next offset dividable by 4;
                w.Write(Audio.SoundData.RawBGM.Length); // BGM
                w.Write((uint)0); // SE1
                w.Write((uint)0); // SE2
                w.Write((uint)0); // SE3
                //w.Write(new byte[(4 - w.BaseStream.Position % 4) % 4]);
                w.Write(Audio.SoundHeader.CurrentFramespeed); // Frame speed
                w.Write(Audio.SoundHeader.RecordingBGMFramespeed); //BGM speed
                w.Write(new byte[14]);


                //write the actual BGM
                w.Write(Audio.SoundData.RawBGM);

                using (var ms = new MemoryStream())
                {
                    var p = w.BaseStream.Position;
                    w.BaseStream.Seek(0, SeekOrigin.Begin);
                    w.BaseStream.CopyTo(ms);
                    w.BaseStream.Seek(p, SeekOrigin.Begin);
                    //sign if you can
                    if (File.Exists("fnkey.pem"))
                    {
                        try
                        {
                            w.Write(ComputeSignature(ms.ToArray()));
                            Signed = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message + "\n" + e.StackTrace);
                            Signed = false;
                        }


                    }
                    else
                    {
                        //placeholder key
                        w.Write(new byte[0x80]);
                        Signed = false;
                    }
                }
                w.Write(new byte[0x10]);

            }
        }

        /// <summary>
        /// Generates the RSA SHA-1 signature for the file data passed as parameter
        /// </summary>
        /// <remarks>
        /// The private key is not contained in this package. Good luck in googling 
        /// it by yourself. Once you have it, place it in a file named "fnkey.pem"
        /// in the root directory.
        /// </remarks>
        /// <param name="data">The PPM binary data</param>      
        /// <returns>a 144-sized byte array.</returns>
        public static byte[] ComputeSignature(byte[] data)
        {
            var privkey = File.ReadAllText("fnkey.pem")
                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace(System.Environment.NewLine, "");
            var rsa = CreateRsaProviderFromPrivateKey(privkey);
            var hash = new SHA1CryptoServiceProvider().ComputeHash(data);
            return rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
        }

        // https://stackoverflow.com/questions/14644926/use-pem-encoded-rsa-private-key-in-net                     
        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privkeybytes = Convert.FromBase64String(privateKey);
            var rsa = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();
            using (BinaryReader r = new BinaryReader(new MemoryStream(privkeybytes)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = r.ReadUInt16();
                if (twobytes == 0x8130)
                    r.ReadByte();
                else if (twobytes == 0x8230)
                    r.ReadInt16();
                else
                    throw new Exception("Unexpected format");

                twobytes = r.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = r.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected format");

                RSAparams.Modulus = r.ReadBytes(GetIntegerSize(r));
                RSAparams.Exponent = r.ReadBytes(GetIntegerSize(r));
                RSAparams.D = r.ReadBytes(GetIntegerSize(r));
                RSAparams.P = r.ReadBytes(GetIntegerSize(r));
                RSAparams.Q = r.ReadBytes(GetIntegerSize(r));
                RSAparams.DP = r.ReadBytes(GetIntegerSize(r));
                RSAparams.DQ = r.ReadBytes(GetIntegerSize(r));
                RSAparams.InverseQ = r.ReadBytes(GetIntegerSize(r));
            }

            rsa.ImportParameters(RSAparams);
            return rsa;
        }

        private static int GetIntegerSize(BinaryReader r)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = r.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = r.ReadByte();
            if (bt == 0x81)
                count = r.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = r.ReadByte();
                lowbyte = r.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
                count = bt;
            while (r.ReadByte() == 0x00)
                count--;
            r.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }




        public void DumpBGMData(string filename)        
            => File.WriteAllBytes(filename, Audio.SoundData.RawBGM);
        public void DumpSE1Data(string filename)
            => File.WriteAllBytes(filename, Audio.SoundData.RawSE1);
        public void DumpSE2Data(string filename)
            => File.WriteAllBytes(filename, Audio.SoundData.RawSE2);
        public void DumpSE3Data(string filename)
            => File.WriteAllBytes(filename, Audio.SoundData.RawSE3);


        internal static readonly char[] FileMagic = new char[4] { 'P', 'A', 'R', 'A' };
        private uint[] _animationOffset;
        public uint AnimationDataSize { get; private set; }
        public uint SoundDataSize { get; private set; }
        public ushort FrameCount { get; private set; } // we should just use Frames.Count() instead
        public ushort FormatVersion { get; private set; }
        public bool IsLocked { get; private set; }
        public ushort ThumbnailFrameIndex { get; private set; }
        public PPMAuthor RootAuthor { get; private set; }
        public PPMAuthor ParentAuthor { get; private set; }
        public PPMAuthor CurrentAuthor { get; private set; }
        public PPMFilename ParentFilename { get; private set; }
        public PPMFilename CurrentFilename { get; private set; }
        public PPMFileFragment RootFileFragment { get; private set; }
        public PPMTimestamp Timestamp { get; private set; }
        public PPMThumbnail Thumbnail { get; private set; }
        public ushort FrameOffsetTableSize { get; private set; }
        public ushort AnimationFlags { get; private set; }
        public PPMFrame[] Frames { get; private set; }
        public byte[] SoundEffectFlags;
        public PPMAudio Audio { get; private set; }
        public byte[] Signature;
        public double Framerate { get; private set; }
        public double BGMRate { get; private set; }

        public double[] PPM_FRAMERATES = new double[]
        {
            30.0,
            0.5,
            1.0,
            2.0,
            4.0,
            6.0,
            12.0,
            20.0,
            30.0
        };

    }
}
