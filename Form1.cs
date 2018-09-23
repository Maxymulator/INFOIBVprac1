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
        private String ContextString = "";



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

            if (ContextString.Equals(""))
            {
                MessageBox.Show("Please select an operation to apply");
            }
            else
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
                switch (ContextString) {
                    //Exercise 1: Applying grayscale
                    case "Grayscale":
                        Image = ApplyGrayScale(Image);
                        break;
                    //Exercise 2: Contrast adjustment
                    case "Contrast adjustment":
                        Image = ApplyContrastAdjustment(Image);
                        break;
                    //Exercise 3 & 4: Create Gaussian filter  Linear filtering
                    case "Gaussian filter":
                        double[,] GaussianKernel = CreateGaussianFilter(11, 3);
                        Image = ApplyLinearFiltering(Image, GaussianKernel);
                        break;
                    //Exercise 5: non-linear filtering:
                    case "Median filter":
                        Image = ApplyNonLinearFiltering(Image, 25);
                        break;
                    //Exercise 6: Edge detection
                    case "Edge detection":
                        Image = ApplyEdgeDetection(Image, Sobelx());
                        break;
                    //Exercise 7: Thresholding
                    case "Thresholding":
                        Image = ApplyThresholding(Image, 20, false);
                        break;
                    //Bonus 1: Histogram equalization
                    case "Histogram equalization":
                        Image = ApplyEqualization(Image);
                        break;
                    //Bonus 2: Edge Sharpening
                    case "Edge sharpening":
                        Image = ApplyEdgeSharpening(Image);
                        break;
                    default:
                        MessageBox.Show("Select a valid option");
                        break;
                }

                
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
        }


        //All Exercise used methods
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
        public Color[,] ApplyContrastAdjustment(Color[,] Image)
        {
            ColorHistogram colorHistogram = new ColorHistogram(Image, InputImage.Size.Height, InputImage.Size.Width);
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
        public Color[,] ApplyLinearFiltering(Color[,] inputImage, double[,] kernel)
        {
            int kernelSize = kernel.GetLength(0);
            int offset = kernelSize / 2;
            int kernel2 = kernel.Length;
            Color[,] newImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
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
        public Color[,] ApplyNonLinearFiltering(Color[,] inputImage, int medianSize)
        {



            int kernelSize = (int)Math.Sqrt(medianSize);
            int median = medianSize / 2;
            int offset = kernelSize / 2;
            Color[,] newImage = new Color[InputImage.Size.Width, InputImage.Size.Height];



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
                            newImage[x, y] = Color.FromArgb((int)Rnew[median], (int)Gnew[median], (int)Bnew[median]);
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
        public Color[,] ColorImageTest(Color[,] Image, int i, int j)
        {
            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    if (x <= 20 && y <= 40)
                    {
                        Image[x, y] = Color.FromArgb(255, 0, 0);
                    }
                }
            }
            return Image;
        }
        public double[,] Sobelx() {
            double[,] Sx = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            return Sx;
        }
        public double[,] Sobely() {
            double[,] Sy = { { -1, 0, 1 }, { 2, 0, 2 }, { -1, 0, 1 } };
            return Sy;
        }
        public double[,] Prewittx() {
            double[,] Px = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };
            return Px;
        }
        public double[,] Prewitty()
        {
            double[,] Py = { { -1, 0, -1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            return Py;
        }
        public Color[,] ApplyEdgeDetection(Color[,] Image, double[,] kernel)
        {
            int kernelSize = kernel.GetLength(0);
            int offset = kernelSize / 2;
            Color[,] newImage = new Color[InputImage.Size.Width,InputImage.Size.Height];
            for (int y = 1; y < InputImage.Size.Height - 1; y++)
            {
                for (int x = 1; x < InputImage.Size.Width - 1; x++)
                {
                    double redx = 0;
                    double grnx = 0;
                    double blux = 0;
                    for (int v = -offset, kj = 0; v <= offset; v++, kj++)
                        for (int u = -offset, ki = 0; u <= offset; u++, ki++)
                        {
                            redx += Image[x + u, y + v].G * kernel[ki, kj];
                            grnx += Image[x + u, y + v].B * kernel[ki, kj];
                            blux += Image[x + u, y + v].R * kernel[ki, kj];
                        }
                    newImage[x, y] = Color.FromArgb((int)Clamp(redx), (int)Clamp(grnx), (int)Clamp(blux));
                }
            }
            return newImage;
        }
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
        public Color[,] ApplyEqualization(Color[,] Image)
        {
            ColorHistogram colorHistogram = new ColorHistogram(Image, InputImage.Size.Height, InputImage.Size.Width);
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
        public Color[,] ApplyEdgeSharpening(Color[,] Image)
        {
            return ApplyEdgeDetection(Image, Laplacian3x3);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ContextString = comboBox1.Text;
        }
    }
}
