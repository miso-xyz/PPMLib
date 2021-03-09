using FFMpegCore;
using FFMpegCore.Enums;
using PPMLib.Extensions;
using PPMLib.Winforms;
using System;
using System.IO;
using System.Linq;

namespace PPMLib.Encoders
{
    public class Mp3Encoder
    {
        private PPMFile Flipnote { get; set; }

        /// <summary>
        /// Simple Mp3 Encoder. Requires FFMpeg to be installed in path.
        /// </summary>
        /// <param name="flipnote"></param>
        public Mp3Encoder(PPMFile flipnote)
        {
            this.Flipnote = flipnote;
        }

        /// <summary>
        /// Encode the Mp3.
        /// </summary>
        /// <returns>Mp3 byte array</returns>
        public byte[] EncodeMp3()
        {
            return Encode();
        }

        /// <summary>
        /// Encode the Mp3 and save it to the specified path
        /// </summary>
        /// <param name="path">Creates path if it doesn't exist. Doesn't save if path is "temp"</param>
        /// <returns>Mp3 byte array</returns>
        public byte[] EncodeMp3(string path)
        {
            return Encode(path);
        }

        /// <summary>
        /// Encode the Mp3 with the specified Samplerate
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns>Mp3 byte array</returns>
        public byte[] EncodeMp3(int sampleRate)
        {
            return Encode("temp", sampleRate);
        }

        /// <summary>
        /// Encode the wav with the specified Samplerate and save it to the given Path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sampleRate"></param>
        /// <returns>Mp3 byte array</returns>
        public byte[] EncodeMp3(string path, int sampleRate)
        {
            return Encode(path, sampleRate);
        }

        private byte[] Encode(string path = "temp", int sampleRate = 32768)
        {
            try
            {
                if (!Directory.Exists("temp"))
                {
                    Directory.CreateDirectory("temp");
                }
                else
                {
                    Cleanup();
                }

                File.WriteAllBytes("temp/audio.wav", Flipnote.Audio.GetWavBGM(Flipnote, sampleRate));

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


                var a = FFMpegArguments
                        .FromFileInput("temp/audio.wav", false)
                        .OutputToFile($"{path}/{Flipnote.CurrentFilename}.mp3", true);

                a.ProcessSynchronously();

                var avi = File.ReadAllBytes($"{path}/{Flipnote.CurrentFilename}.avi");

                Cleanup();

                return avi;


            }
            catch (Exception e)
            {
                Cleanup();
                return null;
            }
        }

        private void Cleanup()
        {
            if (!Directory.Exists("temp"))
            {
                return;
            }
            var files = Directory.EnumerateFiles("temp");

            files.ToList().ForEach(file =>
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    // idk yet
                }

            });
            Directory.Delete("temp");
        }
    }
}
