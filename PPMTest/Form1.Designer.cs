namespace PPMTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Thumbnail = new System.Windows.Forms.PictureBox();
            this.Props = new System.Windows.Forms.PropertyGrid();
            this.FileSel = new System.Windows.Forms.ComboBox();
            this.FrameViewer = new System.Windows.Forms.PictureBox();
            this.NextFrameButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Thumbnail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FrameViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // Thumbnail
            // 
            this.Thumbnail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Thumbnail.Location = new System.Drawing.Point(204, 27);
            this.Thumbnail.Name = "Thumbnail";
            this.Thumbnail.Size = new System.Drawing.Size(66, 50);
            this.Thumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Thumbnail.TabIndex = 0;
            this.Thumbnail.TabStop = false;
            // 
            // Props
            // 
            this.Props.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Props.Location = new System.Drawing.Point(276, 27);
            this.Props.Name = "Props";
            this.Props.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.Props.Size = new System.Drawing.Size(512, 411);
            this.Props.TabIndex = 1;
            this.Props.ToolbarVisible = false;
            this.Props.ViewBackColor = System.Drawing.SystemColors.Control;
            // 
            // FileSel
            // 
            this.FileSel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FileSel.FormattingEnabled = true;
            this.FileSel.Location = new System.Drawing.Point(0, 0);
            this.FileSel.Name = "FileSel";
            this.FileSel.Size = new System.Drawing.Size(800, 21);
            this.FileSel.TabIndex = 2;
            this.FileSel.SelectedIndexChanged += new System.EventHandler(this.FileSel_SelectedIndexChanged);
            // 
            // FrameViewer
            // 
            this.FrameViewer.Location = new System.Drawing.Point(12, 128);
            this.FrameViewer.Name = "FrameViewer";
            this.FrameViewer.Size = new System.Drawing.Size(258, 194);
            this.FrameViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.FrameViewer.TabIndex = 3;
            this.FrameViewer.TabStop = false;
            // 
            // NextFrameButton
            // 
            this.NextFrameButton.Location = new System.Drawing.Point(195, 328);
            this.NextFrameButton.Name = "NextFrameButton";
            this.NextFrameButton.Size = new System.Drawing.Size(75, 23);
            this.NextFrameButton.TabIndex = 4;
            this.NextFrameButton.Text = "Next";
            this.NextFrameButton.UseVisualStyleBackColor = true;
            this.NextFrameButton.Click += new System.EventHandler(this.NextFrameButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.NextFrameButton);
            this.Controls.Add(this.FrameViewer);
            this.Controls.Add(this.FileSel);
            this.Controls.Add(this.Props);
            this.Controls.Add(this.Thumbnail);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Thumbnail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FrameViewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Thumbnail;
        private System.Windows.Forms.ComboBox FileSel;
        private System.Windows.Forms.PropertyGrid Props;
        private System.Windows.Forms.PictureBox FrameViewer;
        private System.Windows.Forms.Button NextFrameButton;
    }
}

