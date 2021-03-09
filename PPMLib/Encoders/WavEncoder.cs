using FFMpegCore;
using FFMpegCore.Enums;
using PPMLib.Extensions;
using PPMLib.Winforms;
using System;
using System.IO;
using System.Linq;

namespace PPMLib.Encoders
{
    public class WavEncoder
    {
        private PPMFile Flipnote { get; set; }

        /// <summary>
        /// Simple Wav Encoder. Requires FFMpeg to be installed in path.
        /// </summary>
        /// <param name="flipnote"></param>
        public WavEncoder(PPMFile flipnote)
        {
            this.Flipnote = flipnote;
        }

        /// <summary>
        /// Encode the Wav.
        /// </summary>
        /// <returns>Wav byte array</returns>
        public byte[] EncodeWav()
        {
            return Encode();
        }

        /// <summary>
        /// Encode the Wav and save it to the specified path
        /// </summary>
        /// <param name="path">Creates path if it doesn't exist. Doesn't save if path is "temp"</param>
        /// <returns>Wav byte array</returns>
        public byte[] EncodeWav(string path)
        {
            return Encode(path);
        }

        /// <summary>
        /// Encode the Wav with the specified Samplerate
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns>Wav byte array</returns>
        public byte[] EncodeWav(int sampleRate)
        {
            return Encode("temp", sampleRate);
        }

        /// <summary>
        /// Encode the wav with the specified Samplerate and save it to the given Path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sampleRate"></param>
        /// <returns>Wav byte array</returns>
        public byte[] EncodeWav(string path, int sampleRate)
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
                        .OutputToFile($"{path}/{Flipnote.CurrentFilename}.wav", true);

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
