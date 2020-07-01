using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MnistDemo
{
    public partial class ImageVisualizer : Form
    {
        public ImageVisualizer(Image img, Image original = null)
        {
            InitializeComponent();
            this.pictureBox1.Image = img;
            if (original != null)
                this.pictureBox2.Image = original;
        }

        private void ImageVisualizer_Load(object sender, EventArgs e)
        {

        }
    }
}
