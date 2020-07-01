using ConvNetSharp.Core;
using ConvNetSharp.Core.Serialization;
using ConvNetSharp.Volume;
using MnistDemo;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MnistTestUi
{
    public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
            var jsonText = File.ReadAllText(@"ConvNetModel.json");
            _net = SerializationExtensions.FromJson<double>(jsonText);
        }

        private Net<double> _net;

        private void openButton_Click(object sender, EventArgs e)
		{
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var jsonText = File.ReadAllText(openFileDialog1.FileName);
                    _net = SerializationExtensions.FromJson<double>(jsonText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid file.");
            }
		}

		private void buttonClear_Click(object sender, EventArgs e)
		{
			drawPanel.Clear();
			multiTokenDrawPanel1.Clear();
			textBoxParsedNumber.Text = "";
			textBoxParsedDigit.Text = "";
		}

		private void button1_Click(object sender, EventArgs e)
		{
            if (_net == null)
            	return;

            var bytes = drawPanel.PreprocessImage(drawPanel.Image, new Size(28,28), true);
			if (bytes != null)
                textBoxParsedDigit.Text = DataSet.MakePrediction(bytes, _net, true);

			var numberBytes = multiTokenDrawPanel1.GetNumberBytes(new Size(28, 28));
			if (numberBytes == null)
				return;
			var sb = new StringBuilder();
			foreach (var byts in numberBytes)
			{
				sb.Append(DataSet.MakePrediction(byts, _net));
            }
            textBoxParsedNumber.Text = sb.ToString();
		}

		private void showButton_Click(object sender, EventArgs e)
		{
			var form = new MnistImagesForm();
            form._net = this._net;
			form.Show(this);
		}
	}
}
