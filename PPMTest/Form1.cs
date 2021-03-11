
using PPMLib;
using PPMLib.Encoders;
using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPMTest
{
    public partial class Form1 : Form
    {

        public static readonly string Path = @"D:\Archive\Files\Nintendo\NDS\flash\R4wood\flipnotes";
        //public static readonly string Path = @"C:\Users\finti\Desktop\";

        public PPMFile ppm;
        public Form1()
        {
            InitializeComponent();
            var ppmfiles = Directory.EnumerateFiles(Path, "*.ppm", SearchOption.TopDirectoryOnly);
            foreach (string fn in ppmfiles)
            {
                FileSel.Items.Add(System.IO.Path.GetFileName(fn));
            }
        }

        public void LoadPPM(string fn)
        {
            ppm = new PPMFile();
            ppm.LoadFrom(System.IO.Path.Combine(Path, fn));
            Props.SelectedObject = ppm;
            Thumbnail.Image = PPMLib.Winforms.PPMRenderer.GetThumbnailBitmap(ppm.Thumbnail.Buffer);
            fcnt = 0;            
        }

        private void FileSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPPM(FileSel.SelectedItem as string);
            fcnt = 0;
            FrameViewer.Image = PPMLib.Winforms.PPMRenderer.GetFrameBitmap(ppm.Frames[fcnt]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileSel.SelectedIndex = 0;
        }

        int fcnt = 0;
        private void NextFrameButton_Click(object sender, EventArgs e)
        {
            FrameViewer.Image = PPMLib.Winforms.PPMRenderer.GetFrameBitmap(ppm.Frames[fcnt]);
            fcnt++;
            if (fcnt >= ppm.FrameCount) fcnt = 0;

        }


        private void PlayButton_Click(object sender, EventArgs e)
        {
            //Return fully mixed audio
            var audio = ppm.Audio.GetWavBGM(ppm);
            using (MemoryStream stream = new MemoryStream(audio))
            {
                SoundPlayer player = new SoundPlayer(stream);
                player.Play();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var mp4 = new Mp3Encoder(ppm);
            //mp4.EncodeMp3("C:/Users/finti/Desktop/EncodedFlipnotes");

            PPMFile aaa = PPMFile.Create(ppm.CurrentAuthor.Name, ppm.CurrentAuthor.Id, ppm.Frames.ToList(), ppm.Audio.SoundData.RawBGM);
            aaa.Save($"{aaa.CurrentFilename}.ppm");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var f = PPMFile.Create();            
        }
    }
}
