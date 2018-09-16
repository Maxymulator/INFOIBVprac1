using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;



        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image)InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // TODO: include here your own code
            // example: create a negative image
            //   Image = ApplyNegation(Image);
            //==========================================================================================
            //Exercise 1: Applying grayscale
            //Image = ApplyGrayScale(Image);
            //Exercise 2: Contrast adjustment
            //Image = ApplyContrastAdjustment(Image);
            //Exercise 4: Linear filtering:
            Image = ApplyLinearFiltering(Image, CreateGaussianFilter(81,9));
            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }

            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }

       



        public Color[,] ApplyNegation(Color[,] Image)
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B); // Negative image
                    Image[x, y] = updatedColor;                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // Increment progress bar
                }
            }
            return Image;
        }

        //Exercise 1: Grayscale conversion
        public Color[,] ApplyGrayScale(Color[,] Image)
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    int R = pixelColor.R;
                    int G = pixelColor.G;
                    int B = pixelColor.B;
                    int Y = (R + G + B) / 3;
                    Color updatedColor = Color.FromArgb(Y, Y, Y);
                    Image[x, y] = updatedColor;
                    progressBar.PerformStep();
                }
            }
            return Image;
        }
        //Exercise 2: Contrast adjustment
        public Color[,] ApplyContrastAdjustment(Color[,] Image)
        {
            ColorHistogram colorHistogram = new ColorHistogram(Image);
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    int Rnew = (pixelColor.R- colorHistogram.ArLow)* (255/(colorHistogram.ArHigh-colorHistogram.ArLow));
                    int Gnew = (pixelColor.G - colorHistogram.AgLow) * (255 / (colorHistogram.AgHigh - colorHistogram.AgLow));
                    int Bnew = (pixelColor.B - colorHistogram.AbLow) * (255 / (colorHistogram.AbHigh - colorHistogram.AbLow));
                    Color updatedColor = Color.FromArgb(Rnew,Gnew,Bnew);
                    Image[x, y] = updatedColor;
                    progressBar.PerformStep();
                }
            }
            return Image;
        }
        //Exercise 3: Gaussian filter
        public double[,] CreateGaussianFilter(int kernelsize, double sigma)
        {
            double[,] kernel = new double[kernelsize, kernelsize];
            int offset = kernelsize / 2;
            for (int y = offset, i = 0; y >= -offset; y--, i++)
                for (int x = -offset, j = 0; x <= offset; x++, j++)
                {
                    double s22 = (2 * (sigma * sigma));
                    double x2 = x * x;
                    double y2 = y * y;
                    double exponent = (x2 + y2) / s22;
                    double p = 1 / (2 * Math.PI * s22);
                    double value = p * Math.Pow(Math.E, -exponent);
                    kernel[i, j] = value;
                }
            double coefficient = 0;
            for (int y = 0; y < kernelsize; y++)
                for (int x = 0; x < kernelsize; x++)
                {
                    coefficient += kernel[x, y];
                }

            for (int y = 0; y < kernelsize; y++)
                for (int x = 0; x < kernelsize; x++)
                {
                    kernel[x, y] = kernel[x, y] / coefficient;
                }
            double test = 0;
            for (int y = 0; y < kernelsize; y++)
                for (int x = 0; x < kernelsize; x++)
                {
                    test += kernel[x, y];
                }
            return kernel;
        }
        //Exercise 4: Linear Filtering
        public Color[,] ApplyLinearFiltering(Color[,] inputImage, double[,] kernel)
        {
            int kernelSize = kernel.GetLength(0);
            int offset = kernelSize / 2;
            int kernel2 = kernel.Length;
            Color[,] newImage = inputImage;
            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                if (y >= offset && y < InputImage.Size.Height - offset)
                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        if (x >= offset && x < InputImage.Size.Width - offset)
                        {
                            double Rnew = 0;
                            double Gnew = 0;
                            double Bnew = 0;
                            //itterate over kernel
                            for (int j = -offset, kj = 0; j <= offset; j++, kj++)
                                for (int i = -offset, ki = 0; i <= offset; i++, ki++)
                                {
                                    Gnew += inputImage[x + i, y + j].G * kernel[ki, kj];
                                    Bnew += inputImage[x + i, y + j].B * kernel[ki, kj];
                                    Rnew += inputImage[x + i, y + j].R * kernel[ki, kj];
                                }
                            newImage[x, y] = Color.FromArgb((int)Rnew, (int)Gnew, (int)Bnew);
                            progressBar.PerformStep();
                        }
                    }
            }
            return newImage;
        }
        //Exercise 5: Nonlinear filtering
        public Color[,] ApplyNonLinearFiltering()
        {
            Color[,] Image = null;

            return Image;
        }
        //Exercise 6: Edge detection
        public Color[,] ApplyEdgeDetection()
        {
            Color[,] Image = null;

            return Image;
        }
        //Exercise 7: Thresholding
        public Color[,] ApplyThresholding()
        {
            Color[,] Image = null;

            return Image;
        }
        //Bonus 1: Histogram equalization
        public Color[,] ApplyEqualization()
        {
            Color[,] Image = null;

            return Image;
        }
        //Bonus 2: Edge sharpening
        public Color[,] ApplyEdgeSharpening()
        {
            Color[,] Image = null;

            return Image;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }
}
