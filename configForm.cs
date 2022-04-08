using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Newtonsoft.Json;

namespace IOCam
{
    public partial class configForm : Form
    {
        private bool firstTime;
        private imageForm imageForm;
        private galleryForm galleryForm;

        public FilterInfoCollection videoDevices;
        public VideoCaptureDevice videoDevice;
        public VideoCapabilities[] videoCapabilities;
        public VideoCapabilities[] snapshotCapabilities;

        public configForm()
        {
            InitializeComponent();
        }

        private void configForm_Load(object sender, EventArgs e)
        {
            firstTime = true;
            galleryForm = null;
            imageForm = null;

            IOCamSettings settings = new IOCamSettings();
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IOCam"))
            {
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IOCam");
            }

            if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IOCam\\config.json"))
            {
                //first run
                firstTime = false;  //allow the config screen to be up and running the first time

                //init default values
                settings.selectedVideo = "";
                settings.selectedResolution = "";
                settings.configScreen = 0;
                settings.liveScreen = 0;
                settings.galleryScreen = 0;

                string settingsJSON = JsonConvert.SerializeObject(settings);
                System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IOCam\\config.json", settingsJSON);
            }

            string jsonText = System.IO.File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IOCam\\config.json");
            settings = JsonConvert.DeserializeObject<IOCamSettings>(jsonText);

            configScreenText.Text = "0";

            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (i == settings.configScreen)
                {
                    configScreenText.Text = i.ToString();
                }

                liveScreenCombo.Items.Add(i.ToString());
                if(i == settings.liveScreen)
                {
                    liveScreenCombo.SelectedIndex = i;
                }

                galleryScreenCombo.Items.Add(i.ToString());
                if (i == settings.galleryScreen)
                {
                    galleryScreenCombo.SelectedIndex = i;
                }
            }

            if(liveScreenCombo.SelectedIndex == -1)
            {
                liveScreenCombo.SelectedIndex = 0;
                firstTime = false;  //show config as option was not found
            }

            if(galleryScreenCombo.SelectedIndex == -1)
            {
                galleryScreenCombo.SelectedIndex = 0;
                firstTime = false;  //show config as option was not found
            }

            this.liveScreenCombo.SelectedIndexChanged += new System.EventHandler(this.liveScreenCombo_SelectedIndexChanged);
            this.galleryScreenCombo.SelectedIndexChanged += new System.EventHandler(this.galleryScreenCombo_SelectedIndexChanged);

            //place window on correct screen
            int configScreen = Convert.ToInt32(configScreenText.Text);
            Left = Screen.AllScreens[configScreen].WorkingArea.Left + Screen.AllScreens[configScreen].WorkingArea.Width/2 - Width/2;

            galleryForm = new galleryForm(this);
            imageForm = new imageForm(this, galleryForm);
            galleryForm.Show();
            imageForm.Show();
            galleryForm.Hide();
            imageForm.Hide();

            enumerateVideoDevices(settings.selectedVideo, settings.selectedResolution);
        }

        private void configForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                if (imageForm.videoSourcePlayer.IsRunning)
                {
                    imageForm.videoSourcePlayer.SignalToStop();
                    imageForm.videoSourcePlayer.WaitForStop();
                    imageForm.videoSourcePlayer.VideoSource = null;
                }

                IOCamSettings settings = new IOCamSettings();
                settings.selectedVideo = videoDevicesCombo.SelectedItem.ToString();
                settings.selectedResolution = snapshotCapabilitiesCombo.SelectedItem.ToString();
                settings.configScreen = Convert.ToInt32(configScreenText.Text);
                settings.liveScreen = liveScreenCombo.SelectedIndex;
                settings.galleryScreen = galleryScreenCombo.SelectedIndex;

                string settingsJSON = JsonConvert.SerializeObject(settings);
                System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IOCam\\config.json", settingsJSON);

                imageForm = null;
            }
        }

        private void configForm_Move(object sender, EventArgs e)
        {
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if ((Screen.AllScreens[i].WorkingArea.Left <= Left) && (Screen.AllScreens[i].WorkingArea.Right >= Left + Width))
                {
                    configScreenText.Text = i.ToString();
                }
            }
        }

        private void configForm_Shown(object sender, EventArgs e)
        {
            if (firstTime)
            {
                Hide();
                firstTime = true;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.axiplus.com");
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            enumerateVideoDevices("", "");
        }

        public void enumerateVideoDevices(string selectedVideo, string selectedResolution)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            videoDevicesCombo.Items.Clear();
            videoDevicesCombo.Items.Add("Select camera...");
            videoDevicesCombo.SelectedIndex = 0;

            snapshotCapabilitiesCombo.Items.Clear();
            snapshotCapabilitiesCombo.Items.Add("Select resolution...");
            snapshotCapabilitiesCombo.SelectedIndex = 0;

            if (videoDevices.Count != 0)
            {
                // add all devices to combo
                foreach (FilterInfo device in videoDevices)
                {
                    videoDevicesCombo.Items.Add(device.Name);
                }
            }

            bool indexSelected = false;
            for(int i = 0; i < videoDevicesCombo.Items.Count; i++)
            {
                if(videoDevicesCombo.Items[i].ToString() == selectedVideo)
                {
                    videoDevicesCombo.SelectedIndex = i;
                    indexSelected = true;
                }
            }
            
            if(indexSelected == false)
            {
                videoDevicesCombo.SelectedIndex = 0;
                firstTime = false;  //show config as option was not found
            }
        }

        private void videoDevicesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(imageForm.videoSourcePlayer.IsRunning)
            {
                imageForm.videoSourcePlayer.SignalToStop();
                imageForm.videoSourcePlayer.WaitForStop();
                imageForm.videoSourcePlayer.VideoSource = null;
            }

            snapshotCapabilitiesCombo.Items.Clear();
            snapshotCapabilitiesCombo.Items.Add("Select resolution...");
            snapshotCapabilitiesCombo.SelectedIndex = 0;

            if (videoDevicesCombo.SelectedIndex == 0)
            {
                return;
            }

            statusLabel.Text = "";
            saveButton.Enabled = true;

            if (videoDevices.Count != 0)
            {
                videoDevice = new VideoCaptureDevice(videoDevices[videoDevicesCombo.SelectedIndex - 1].MonikerString);
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    videoCapabilities = videoDevice.VideoCapabilities;
                    snapshotCapabilities = videoDevice.SnapshotCapabilities;
                    if (snapshotCapabilities.Length == 0)
                    {
                        statusLabel.Text = videoDevicesCombo.Text + " is not a supported IO camera.";
                        saveButton.Enabled = false;
                    }
                    else
                    {
                        foreach (VideoCapabilities capabilty in snapshotCapabilities)
                        {
                            snapshotCapabilitiesCombo.Items.Add(string.Format("{0} x {1}", capabilty.FrameSize.Width, capabilty.FrameSize.Height));
                        }

                        snapshotCapabilitiesCombo.SelectedIndex = snapshotCapabilitiesCombo.Items.Count - 1;
                    }
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void videoCapabilitiesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (snapshotCapabilitiesCombo.SelectedIndex == 0)
            {
                return;
            }

            if (videoDevice != null)
            {
                
                if ((videoCapabilities != null) && (videoCapabilities.Length != 0))
                {
                    int videoCapability = 0;
                    for (int i = 0; i < videoCapabilities.Length; i++)
                    {
                        if (videoCapabilities[i].FrameSize == snapshotCapabilities[snapshotCapabilitiesCombo.SelectedIndex - 1].FrameSize)
                        {
                            videoCapability = i;
                        }
                    }
                    
                    videoDevice.VideoResolution = videoCapabilities[videoCapability];
                }

                if ((snapshotCapabilities != null) && (snapshotCapabilities.Length != 0))
                {
                    videoDevice.ProvideSnapshots = true;
                    videoDevice.SnapshotResolution = snapshotCapabilities[snapshotCapabilitiesCombo.SelectedIndex - 1];
                    videoDevice.SnapshotFrame += new NewFrameEventHandler(imageForm.videoDevice_SnapshotFrame);
                }

                imageForm.videoSourcePlayer.VideoSource = videoDevice;
                imageForm.videoSourcePlayer.Start();
            }
        }

        private void liveScreenCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (imageForm != null)
            {
                imageForm.moveToScreen(liveScreenCombo.SelectedIndex);
            }
        }

        private void galleryScreenCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (galleryForm != null)
            {
                galleryForm.moveToScreen(galleryScreenCombo.SelectedIndex);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void galleryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            galleryForm.Show();
        }
    }

    public class IOCamSettings
    {
        public string selectedVideo
        {
            get;
            set;
        }

        public string selectedResolution
        {
            get;
            set;
        }

        public int configScreen
        {
            get;
            set;
        }

        public int liveScreen
        {
            get;
            set;
        }

        public int galleryScreen
        {
            get;
            set;
        }
    }
}
