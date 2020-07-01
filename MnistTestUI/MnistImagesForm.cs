using ConvNetSharp.Core;
using MnistDemo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MnistTestUi
{
    public partial class MnistImagesForm : Form
    {
        DataSets datasets = new DataSets();

        public MnistImagesForm()
		{
			InitialzeCheckBoxes();

			InitializeComponent();

            if (!datasets.Load())
                return;

            ShowImages();
			panel1.Paint += Panel1_Paint;
			comboBox1.SelectedIndex = 0;
		}

		private void InitialzeCheckBoxes()
		{
			SuspendLayout();
			for (int i = 0; i < 10; i++)
			{
				var check = new CheckBox
				{
					AutoSize = true,
					Location = new Point(300 + 100 * i, 13),
					TabIndex = i,
					Name = $"checkBox{i}",
					Text = i.ToString(),
					Checked = true,
					Visible = true,
					UseVisualStyleBackColor = true,
					Tag = i
				};
				this.Controls.Add(check);
				check.CheckedChanged += Check_CheckedChanged;
			}
			ResumeLayout(true);
		}

		private void Check_CheckedChanged(object sender, EventArgs e)
		{
			ShowImages();
			panel1.Invalidate();
		}

		private void ShowImages()
		{
			var checks = Controls.OfType<CheckBox>().Where(x => x.Checked).Select(c => (int)c.Tag).ToArray();
			var imgCountX = 47;
			var imgCountY = 27;
			var start = (int)(numericUpDown1.Value - 1) * (imgCountX * imgCountY);
			var samples = GetSamples();
            var filteredSamples = samples._trainImages.Where(m => checks.Contains(m.Label));
			var imgData = filteredSamples.Skip(start).Select(t => t).ToArray();
			var image = new DirectBitmap(imgCountX * 28, imgCountY * 28);
			var even = true;

            for (int j = 0; j < imgCountY; j++)
			{
				for (int i = 0; i < imgCountX; i++)
				{
					if (i + j * 28 > imgData.Length - 1)
						continue;

					for (int y = 0; y < 28; y++)
					{
						for (int x = 0; x < 28; x++)
						{
							var b = (int)(imgData[i + j * 28].Image[y * 28 + x]);
							if (b == 0 && even)
								image.SetPixel(x + i * 28, y + j * 28, Color.White);
							else
								image.SetPixel(x + i * 28, y + j * 28, Color.FromArgb(b, Color.Black));
						}
					}
					even = !even;
                }
			}
			numericUpDown1.Maximum = (filteredSamples.Count() / (imgCountX * imgCountY)) + 1;
			labelPageCount.Text = "of " + numericUpDown1.Maximum;
			Bitmap?.Dispose();
			Bitmap = image;
		}

		private DataSet GetSamples()
		{
			if (comboBox1.SelectedIndex == 2)
			{
				GetUnknowns();
                return Unknowns;
			}
			else
			{
                var samps = comboBox1.SelectedIndex == 0 ? datasets.Train : datasets.Test;
				return samps;
			}
		}

		private async void GetUnknowns()
		{
			if (Unknowns == null)
			{
				Unknowns = new DataSet(new List<MnistEntry>());
				MessageBox.Show("Click Ok to run tests! It will take a minute.");
				progressBar1.Visible = true;
				await Task.Run(() => {
					var count = datasets.Test._trainImages.Count;

					for (int i = 0; i < count; i++)
					{
                        var bytes = datasets.Test._trainImages[i].Image;
                        String predicted = DataSet.MakePrediction(bytes, _net);
                        MnistEntry entry = new MnistEntry();
                        entry.Image = bytes;
                        entry.Label = Convert.ToInt32(predicted);

                        if (datasets.Test._trainImages[i].Label != entry.Label)
                        {
                            Unknowns._trainImages.Add(entry);
                            Debug.WriteLine("Label: " + datasets.Test._trainImages[i].Label + " Predicted: " + predicted);
                        }
						if (i % 50 == 0)
							SetProgress(i / (double)count);
					}
				});
				ShowImages();
				progressBar1.Visible = false;

				panel1.Invalidate();
			}
		}

		delegate void SetProgressCallback(double progress);

		private void SetProgress(double progress)
		{
			if (progressBar1.InvokeRequired)
			{
				SetProgressCallback d = new SetProgressCallback(SetProgress);
				this.Invoke(d, new object[] { progress });
			}
			else
			{
				progressBar1.Value = (int)(progressBar1.Maximum * progress);
			}
		}

		private DataSet Unknowns { get; set; }

		private DirectBitmap Bitmap { get; set; }
        internal Net<double> _net;

        private void Panel1_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(Bitmap.Bitmap, 0, 0);

		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			ShowImages();
			panel1.Invalidate();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ShowImages();
			panel1.Invalidate();
		}
	}
}
