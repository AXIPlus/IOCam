using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;


namespace IOCam
{
    public partial class imageForm : Form
    {
        private galleryForm galleryForm;
        private configForm configForm;
        private int frames;

        
        public imageForm(configForm configForm, galleryForm galleryForm)
        {
            frames = 0;
            this.configForm = configForm;
            this.galleryForm = galleryForm;
            InitializeComponent();
        }

        private void imageForm_Load(object sender, EventArgs e)
        {
            //place window on correct screen
            int liveScreen = Convert.ToInt32(configForm.liveScreenCombo.SelectedItem.ToString());
            moveToScreen(liveScreen);

            Hide();   
        }

        public void moveToScreen(int liveScreen)
        {
            Top = 0;
            Height = Screen.AllScreens[liveScreen].WorkingArea.Height;
            Left = Screen.AllScreens[liveScreen].WorkingArea.Left;
            Width = Screen.AllScreens[liveScreen].WorkingArea.Width;
        }

        private void imageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configForm.Show();
        }

        private void galleryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            galleryForm.Show();
        }

        private void imageForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.C)
            {
                configForm.Show();
            }

            if (e.KeyCode == Keys.G)
            {
                galleryForm.Show();
            }
        }

        public void videoDevice_SnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            addSnapshot((Bitmap)eventArgs.Frame.Clone());
        }

        private void addSnapshot(Bitmap bitmap)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Bitmap>(galleryForm.addSnapshot), bitmap);
            }
            else
            {
                galleryForm.addSnapshot(bitmap);
            }
        }

        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            frames++;
        }

        private void frameTimer_Tick(object sender, EventArgs e)
        {
            if((frames == 0) && Visible)
            {
                Hide();
            }
            else if ((frames != 0) && !Visible)
            {
                Show();
            }

            frames = 0;
        }

        
    }
}
