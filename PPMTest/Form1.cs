
using PPMLib;
using System;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace PPMTest
{
    public partial class Form1 : Form
    {

        public static readonly string Path = @"C:\Users\finti\Desktop\";
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
            ppm.LoadFromFile(System.IO.Path.Combine(Path, fn));
            Props.SelectedObject = ppm;
            Thumbnail.Image = PPMRenderer.GetThumbnailBitmap(ppm.Thumbnail.Buffer);
            fcnt = 0;
            //for (int i = 0; i < ppm.FramesCount; i++)
            //  ppm.Frames[i].ToBitmap().Save($"{i}.png");         */
        }

        private void FileSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPPM(FileSel.SelectedItem as string);
            fcnt = 0;
            FrameViewer.Image = PPMRenderer.GetFrameBitmap(ppm.Frames[fcnt]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileSel.SelectedIndex = 0;
            //MessageBox.Show(string.Join(" ", new Filename18(ppm.ParentFilename.ToString()).Bytes.Select(x => x.ToString("X2"))));
            //MessageBox.Show(string.Join(" ", new Filename18(ppm.CurrentFilename.ToString()).Bytes.Select(x => x.ToString("X2"))));
            //MessageBox.Show(ppm.ParentFilename.Bytes.Length.ToString());
        }

        int fcnt = 0;
        private void NextFrameButton_Click(object sender, EventArgs e)
        {
            FrameViewer.Image = PPMRenderer.GetFrameBitmap(ppm.Frames[fcnt]);
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




    }
}
