using FFMpegCore;
using FFMpegCore.Enums;
using PPMLib.Extensions;
using PPMLib.Winforms;
using System;
using System.IO;
using System.Linq;

namespace PPMLib.Encoders
{
    public class Mp4Encoder
    {
        private PPMFile Flipnote { get; set; }

        public Mp4Encoder(PPMFile flipnote)
        {
            this.Flipnote = flipnote;
        }

        public byte[] EncodeMp4()
        {
            return Encode();
        }

        public byte[] EncodeMp4(string path)
        {
            return Encode(path);
        }

        public byte[] EncodeMp4(int scale)
        {
            return Encode("out", scale);
        }

        public byte[] EncodeMp4(string path, int scale)
        {
            return Encode(path, scale);
        }

        private byte[] Encode(string path = "out", int scale = 1)
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

                for (int i = 0; i < Flipnote.FrameCount; i++)
                {
                    PPMRenderer.GetFrameBitmap(Flipnote.Frames[i]).Save($"temp/frame_{i}.png");
                }
                var frames = Directory.EnumerateFiles("temp").ToArray();


                File.WriteAllBytes("temp/audio.wav", Flipnote.Audio.GetWavBGM(Flipnote));

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }




                Utils.NumericalSort(frames);

                var a = FFMpegArguments
                        .FromConcatInput(frames, options => options
                        .WithFramerate(Flipnote.Framerate))
                        .AddFileInput("temp/audio.wav", false)
                        .OutputToFile($"{path}/{Flipnote.CurrentFilename}.mp4", true, o => 
                        {
                            o.Resize(256 * scale, 192 * scale)
                            .WithVideoCodec(VideoCodec.LibX264)
                            .ForcePixelFormat("yuv420p")
                            .ForceFormat("mp4");
                        });

                a.ProcessAsynchronously().Wait();

                Cleanup();

                return File.ReadAllBytes($"{path}/{Flipnote.CurrentFilename}.mp4");


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
