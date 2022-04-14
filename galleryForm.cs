using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOCam
{
    public partial class galleryForm : Form
    {
        private configForm configForm;
        private previewForm previewForm;
        private ArrayList snapshots;

        public galleryForm(configForm configForm)
        {
            this.configForm = configForm;
            this.snapshots = new ArrayList();
            InitializeComponent();
        }

        private void galleryForm_Load(object sender, EventArgs e)
        {
            //place window on correct screen
            int galleryScreen = Convert.ToInt32(configForm.galleryScreenCombo.SelectedItem.ToString());
            moveToScreen(galleryScreen);

            previewForm = new previewForm();
            Hide();
        }

        public void moveToScreen(int galleryScreen)
        {
            Top = 0;
            Left = Screen.AllScreens[galleryScreen].WorkingArea.Right - Width;
            Height = Screen.AllScreens[galleryScreen].WorkingArea.Height;
        }

        private void galleryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                clearSnapshots();
                Hide();
            }
        }

        public void addSnapshot(Bitmap bitmap)
        {
            lock (this)
            {
                snapshotItem snapshot = new snapshotItem(DateTime.Now.ToLocalTime(), bitmap);
                snapshot.pictureBox.Location = new System.Drawing.Point(0, 88 + (snapshot.pictureBox.Height + 16) * snapshots.Count);

                snapshot.pictureBox.MouseDown += pictureBox_MouseDown;
                snapshot.pictureBox.MouseEnter += pictureBox_MouseEnter;
                snapshot.pictureBox.MouseLeave += pictureBox_MouseLeave;

                snapshots.Add(snapshot);
                mainPanel.Controls.Add(snapshot.pictureBox);
            }

            if (Visible == false)
            {
                Show();
            }
        }

        public void removeSnapshot(string path)
        {
            int index = 0;
            for (int i = 0; i < snapshots.Count; i++)
            {
                if(((snapshotItem)snapshots[i]).path == path)
                {
                    index = i;
                    break;
                }
            }

            removeSnapshot(index);
        }

        public void removeSnapshot(int index)
        {
            int y = ((snapshotItem)snapshots[index]).pictureBox.Height + 16;
            for(int i = index + 1; i < snapshots.Count; ++i)
            {
                ((snapshotItem)snapshots[i]).pictureBox.Top -= y;
            }

            mainPanel.Controls.Remove(((snapshotItem)snapshots[index]).pictureBox);
            ((snapshotItem)snapshots[index]).Dispose();
            
            snapshots.RemoveAt(index);

            saveButton.Visible = false;
            deleteButton.Visible = false;
        }

        private void clearSnapshots()
        {
            while (snapshots.Count > 0)
            {
                removeSnapshot(0);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            clearSnapshots();
        }

        private void saveAllPicture_MouseDown(object sender, MouseEventArgs e)
        {
            string[] files = new string[snapshots.Count];
            for(int i = 0; i < snapshots.Count; i++)
            {
                files[i] = ((snapshotItem)snapshots[i]).path;
            }
            DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ((PictureBox)sender).ContextMenuStrip.Tag = ((PictureBox)sender).Tag.ToString();
                ((PictureBox)sender).ContextMenuStrip.Show(Cursor.Position);
            }
            else
            {
                string[] files = new string[] { ((PictureBox)sender).Tag.ToString() };
                DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy);
            }
        }

        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            Bitmap old = (Bitmap)previewForm.pictureBox.Image;
            previewForm.pictureBox.Image = ((PictureBox)sender).Image;
            previewForm.Size = previewForm.pictureBox.Image.Size;
            previewForm.Location = new Point(Left - previewForm.pictureBox.Image.Width, 0);

            previewForm.Show();

            saveButton.Tag = ((PictureBox)sender).Tag.ToString();
            deleteButton.Tag = ((PictureBox)sender).Tag.ToString();

            saveButton.Top = ((PictureBox)sender).Top + 4;
            deleteButton.Top = ((PictureBox)sender).Top + 4;

            saveButton.Left = ((PictureBox)sender).Left + 4;
            deleteButton.Left = ((PictureBox)sender).Left + ((PictureBox)sender).Width - 4 - deleteButton.Width;

            saveButton.Visible = true;
            deleteButton.Visible = true;
        }

        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            previewForm.Hide();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            removeSnapshot(((Button)sender).Tag.ToString());
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(((Button)sender).Tag.ToString(), saveFileDialog.FileName);
            }
        }
    }
}

class PanelNoScrollOnFocus : Panel
{
    protected override System.Drawing.Point ScrollToControl(Control activeControl)
    {
        return DisplayRectangle.Location;
    }
}

public class snapshotItem: IDisposable
{
    public string name;
    public string date;
    public PictureBox pictureBox;

    public string path;

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        foreach (var codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

    public snapshotItem(DateTime datetime, Bitmap bitmap)
    {
        this.name = datetime.ToString("yyyyMMddHHmmssfff");
        this.date = datetime.ToString("yyyy-MM-dd HH:mm:ss");
        path = Path.GetTempPath() + name + ".jpg";

        using (Graphics g = Graphics.FromImage(bitmap))
        {
            Image image = IOCam.Properties.Resources.ad_white;
            g.DrawImage(image, new Rectangle(bitmap.Width - image.Width - 20, bitmap.Height - image.Height - 60, image.Width, image.Height));
           
            using (Font arialFont = new Font("Arial", 14))
            {
                SizeF s = g.MeasureString(date, arialFont);
                PointF textLocation = new PointF(bitmap.Width - image.Width/2 - s.Width/2 - 20, bitmap.Height - s.Height - 30);
                g.DrawString(date, arialFont, Brushes.White, textLocation);

                string userName = Environment.MachineName + "/" + Environment.UserName;
                s = g.MeasureString(userName, arialFont);
                textLocation = new PointF(bitmap.Width - image.Width / 2 - s.Width / 2 - 20, bitmap.Height - s.Height - 10);
                g.DrawString(userName, arialFont, Brushes.White, textLocation);
            }
        }

        pictureBox = new PictureBox
        {
            Image = bitmap,
            Tag = path,

            Size = new System.Drawing.Size(370, 212),
            SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage,
            TabIndex = 0,
            TabStop = false
        };

        var encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
        pictureBox.Image.Save(path, GetEncoder(ImageFormat.Jpeg), encoderParameters);
    }

    public void Dispose()
    {
        File.Delete(path);
    }

    ~snapshotItem()
    {
        Dispose();
    }
}
