using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MnistDemo
{
    public class ImageTransformation
    {
        public List<MnistEntry> DataAugmentation(List<MnistEntry> train_images)
        {
            List<MnistEntry> new_images = new List<MnistEntry>();
            Random rnd = new Random();
            for (int i = 0; i <= train_images.Count - 1; i++)
            {
                MnistEntry mnistEntry = train_images[i];
                Image img = byteArrayToImage(mnistEntry.Image);

                //Translation only
                Bitmap newImage = TranslateImage(img, 1, 0);
                AddNewImage(new_images, newImage, mnistEntry.Label);

                Bitmap newImage2 = TranslateImage(img, -1, 0);
                AddNewImage(new_images, newImage2, mnistEntry.Label);

                Bitmap newImage3 = TranslateImage(img, 0, 1);
                AddNewImage(new_images, newImage3, mnistEntry.Label);

                Bitmap newImage4 = TranslateImage(img, 0, -1);
                AddNewImage(new_images, newImage4, mnistEntry.Label);

                //Program.ShowImageVisualizer(byteArrayToImage(newImage), img);

            }
            train_images.AddRange(new_images);
            return train_images;
        }

        void AddNewImage(List<MnistEntry> new_images, Bitmap newImage, int label)
        {
            MnistEntry mnistNewEntry = new MnistEntry();
            mnistNewEntry.Image = ToByteArray(newImage);
            mnistNewEntry.Label = label;
            new_images.Add(mnistNewEntry);
        }

        public byte[] ToByteArray(Bitmap bitmap)
        {
            var bytes = new List<byte>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    byte rgb = color.R;
                    bytes.Add(rgb);
                }
            }
            return bytes.ToArray();
        }

        Bitmap RotateImage(Image img, Boolean clockwise)
        {
            Bitmap newImage = new Bitmap(img.Width, img.Height);
            using (Graphics gfx = Graphics.FromImage(newImage))
            {
                gfx.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);
                if (clockwise)
                    gfx.RotateTransform(5.0F);
                else
                    gfx.RotateTransform(-5.0F);

                gfx.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);
                gfx.DrawImage(img, new Point(0, 0));
            }
            return newImage;
        }

        Bitmap TranslateImage(Image img, int right, int up)
        {
            int movement = 1;
            Bitmap newImage = new Bitmap(img.Width, img.Height);
            using (Graphics gfx = Graphics.FromImage(newImage))
            {
                if (right > 0)
                    gfx.TranslateTransform(movement, 0);
                else if (right < 0)
                    gfx.TranslateTransform(-movement, 0);

                if (up > 0)
                    gfx.TranslateTransform(0, movement);
                else if (up < 0)
                    gfx.TranslateTransform(0, -movement);

                gfx.DrawImage(img, new Point(0, 0));
            }
            return newImage;
        }
        Bitmap ZoomImage(Image img, Boolean zoomIn)
        {
            float zoomFactor = zoomIn ? 1.1f : 0.9f;

            RectangleF recDest = new RectangleF((img.Width - img.Width * zoomFactor) * 0.5f, (img.Height - img.Height * zoomFactor) * 0.5f, img.Width * zoomFactor, img.Height * zoomFactor);
            RectangleF recSrc = new RectangleF(0.0f, 0.0f, img.Width, img.Height);

            Bitmap newImage = new Bitmap(img.Width, img.Height);
            using (Graphics gfx = Graphics.FromImage(newImage))
            {
                gfx.DrawImage(img, recDest, recSrc, GraphicsUnit.Pixel);
            }
            return newImage;
        }


        public Image byteArrayToImage(byte[] imgData)
        {
            Bitmap image = new Bitmap(28, 28);
            for (int y = 0; y < 28; y++)
            {
                for (int x = 0; x < 28; x++)
                {
                    var b = (int)(imgData[y * 28 + x]);
                    if (b == 0)
                        image.SetPixel(x, y, Color.Black);
                    else
                        image.SetPixel(x, y, Color.FromArgb(255, b, b, b));
                }
            }
            return image;
        }

    }
}
