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
            //Image = ApplyLinearFiltering(Image, CreateGaussianFilter(81,9));

            //Exercise 5: non-linear filtering:
            //Image = ApplyNonLinearFiltering(Image, 4);

            //Exercise 6: Edge detection
            Image = ApplyEdgeDetection(Image, Laplacian3x3);

            //Exercise 7: Thresholding
            //Image = ApplyThresholding(Image, 150, false);

            //Bonus 1: Histogram equalization
            //Image = ApplyEqualization(Image);

            //Bonus 2:
            //Image =

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



        public Color[,] CutEdges(Color[,] Image, int borderSize)
        {
            Color[,] newImage = new Color[InputImage.Size.Width, InputImage.Size.Height];

            for (int y = 0; y < InputImage.Size.Height; y++)
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    if (x < InputImage.Size.Width - (borderSize * 2))
                        if (y < InputImage.Size.Height - (borderSize * 2))
                            newImage[x, y] = Image[x + borderSize, y + borderSize];
                }
            return newImage;
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
                    int Rnew = (pixelColor.R - colorHistogram.ArLow) * (255 / (colorHistogram.ArHigh - colorHistogram.ArLow));
                    int Gnew = (pixelColor.G - colorHistogram.AgLow) * (255 / (colorHistogram.AgHigh - colorHistogram.AgLow));
                    int Bnew = (pixelColor.B - colorHistogram.AbLow) * (255 / (colorHistogram.AbHigh - colorHistogram.AbLow));
                    Color updatedColor = Color.FromArgb(Rnew, Gnew, Bnew);
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
                            for (int v = -offset, kj = 0; v <= offset; v++, kj++)
                                for (int u = -offset, ki = 0; u <= offset; u++, ki++)
                                {
                                    Gnew += inputImage[x + u, y + v].G * kernel[ki, kj];
                                    Bnew += inputImage[x + u, y + v].B * kernel[ki, kj];
                                    Rnew += inputImage[x + u, y + v].R * kernel[ki, kj];
                                }

                            Rnew = Clamp(Rnew);
                            Gnew = Clamp(Gnew);
                            Bnew = Clamp(Bnew);

                            newImage[x, y] = Color.FromArgb((int)Rnew, (int)Gnew, (int)Bnew);
                            progressBar.PerformStep();
                        }
                    }
            }
            newImage = CutEdges(newImage, offset);
            return newImage;
        }

        public double Clamp(double value)
        {
            if (value < 0) value = 0;
            else if (value > 255) value = 255;
            return value;
        }

        //Exercise 5: Nonlinear filtering
        public Color[,] ApplyNonLinearFiltering(Color[,] inputImage, int medianSize)
        {



            int kernelSize = (int)Math.Sqrt((medianSize * 2) + 1);
            int offset = kernelSize / 2;
            Color[,] newImage = inputImage;



            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                if (y >= offset && y < InputImage.Size.Height - offset)
                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        if (x >= offset && x < InputImage.Size.Width - offset)
                        {

                            double[] Rnew = new double[kernelSize * kernelSize];
                            double[] Gnew = new double[kernelSize * kernelSize];
                            double[] Bnew = new double[kernelSize * kernelSize];
                            //itterate over kernel
                            int k = 0;
                            for (int j = -offset, kj = 0; j <= offset; j++, kj++)
                                for (int i = -offset, ki = 0; i <= offset; i++, ki++)
                                {
                                    Rnew[k] = (double)inputImage[x + i, y + j].R;
                                    Gnew[k] = (double)inputImage[x + i, y + j].G;
                                    Bnew[k] = (double)inputImage[x + i, y + j].B;
                                    k++;
                                }
                            Array.Sort(Rnew);
                            Array.Sort(Gnew);
                            Array.Sort(Bnew);
                            newImage[x, y] = Color.FromArgb((int)Rnew[medianSize], (int)Gnew[medianSize], (int)Bnew[medianSize]);
                            progressBar.PerformStep();
                        }
                    }
            }
            return newImage;
        }

        public static double[,] Laplacian3x3
        {
            get
            {
                return new double[,]
                {   { -1, -1, -1, },
                    { -1,  8, -1, },
                    { -1, -1, -1, }, };
            }
        }

        //double[,] kernel = new double[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
        //Exercise 6: Edge detection
        public Color[,] ApplyEdgeDetection(Color[,] Image, double[,] kernel)
        {
            int kernelSize = kernel.GetLength(0);
            int offset = kernelSize / 2;
            Color[,] newImage = Image, newImageX = Image, newImageY = Image;

            //calc x image
            for (int y = 0; y < InputImage.Size.Height; y++)
                if (y >= offset && y < InputImage.Size.Height - offset)
                    for (int x = 0; x < InputImage.Size.Width; x++)
                        if (x >= offset && x < InputImage.Size.Width - offset)
                        {
                            double red = 0;//= Image[x, y - 1].R + Image[x + 1, y].R + Image[x, y + 1].R + Image[x - 1, y].R - 4 * Image[x, y].R;
                            double grn = 0;// Image[x, y - 1].G + Image[x + 1, y].G + Image[x, y + 1].G + Image[x - 1, y].G - 4 * Image[x, y].G;
                            double blu = 0;//= Image[x, y - 1].B + Image[x + 1, y].B + Image[x, y + 1].B + Image[x - 1, y].B - 4 * Image[x, y].B;

                            double[,] kernelX = new double[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
                            double[,] kernelY = new double[,] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

                            // calc x
                            for (int v = -offset, kv = 0; v <= offset; v++, kv++)
                                for (int u = -offset, ku = 0; u <= offset; u++, ku++)
                                {
                                    red += Image[x + u, y + v].G * kernelX[ku, kv];
                                    grn += Image[x + u, y + v].B * kernelX[ku, kv];
                                    blu += Image[x + u, y + v].R * kernelX[ku, kv];
                                }

                            red /= 6;
                            grn /= 6;
                            blu /= 6;

                            red = Clamp(red);
                            grn = Clamp(grn);
                            blu = Clamp(blu);

                            newImageX[x, y] = Color.FromArgb((int)red, (int)grn, (int)blu);

                            // calc y
                            for (int v = -offset, kv = 0; v <= offset; v++, kv++)
                                for (int u = -offset, ku = 0; u <= offset; u++, ku++)
                                {
                                    red += Image[x + u, y + v].G * kernelY[ku, kv];
                                    grn += Image[x + u, y + v].B * kernelY[ku, kv];
                                    blu += Image[x + u, y + v].R * kernelY[ku, kv];
                                }

                            red /= 6;
                            grn /= 6;
                            blu /= 6;

                            red = Clamp(red);
                            grn = Clamp(grn);
                            blu = Clamp(blu);

                            newImageY[x, y] = Color.FromArgb((int)red, (int)grn, (int)blu);

                            red = Math.Sqrt((newImageX[x, y].R * newImageX[x, y].R) + (newImageY[x, y].R * newImageY[x, y].R));
                            grn = Math.Sqrt((newImageX[x, y].G * newImageX[x, y].G) + (newImageY[x, y].G * newImageY[x, y].G));
                            blu = Math.Sqrt((newImageX[x, y].B * newImageX[x, y].B) + (newImageY[x, y].B * newImageY[x, y].B));

                            red = Clamp(red);
                            grn = Clamp(grn);
                            blu = Clamp(blu);

                            newImage[x, y] = Color.FromArgb((int)red, (int)grn, (int)blu);
                        }
            return newImageX;
        }
        //Exercise 7: Thresholding
        public Color[,] ApplyThresholding(Color[,] Image, int threshold, bool invert)
        {
            Color[,] newImage = Image;
            int pixelValue;
            Image = ApplyGrayScale(Image);

            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    pixelValue = Image[x, y].R;

                    if (pixelValue < threshold && !invert)
                        newImage[x, y] = Color.Black;
                    else if (pixelValue >= threshold && !invert)
                        newImage[x, y] = Color.White;
                    else if (pixelValue < threshold && invert)
                        newImage[x, y] = Color.White;
                    else
                        newImage[x, y] = Color.Black;
                    progressBar.PerformStep();
                }
            }
            return newImage;
        }
        //Bonus 1: Histogram equalization
        public Color[,] ApplyEqualization(Color[,] Image)
        {
            ColorHistogram colorHistogram = new ColorHistogram(Image);
            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    int R = pixelColor.R;
                    int G = pixelColor.G;
                    int B = pixelColor.B;
                    double _255_MN = (double)255 / (InputImage.Size.Width * InputImage.Size.Height);
                    double newR = colorHistogram.CummulativeHistogramR[R] * _255_MN;
                    double newG = colorHistogram.CummulativeHistogramG[G] * _255_MN;
                    double newB = colorHistogram.CummulativeHistogramB[B] * _255_MN;
                    Color updatedColor = Color.FromArgb((int)newR, (int)newG, (int)newB);
                    Image[x, y] = updatedColor;
                    progressBar.PerformStep();

                }
            }
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
