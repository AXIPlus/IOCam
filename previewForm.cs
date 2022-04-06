using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOCam
{
    public partial class previewForm : Form
    {
        public previewForm()
        {
            InitializeComponent();
        }
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private void previewForm_Load(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
