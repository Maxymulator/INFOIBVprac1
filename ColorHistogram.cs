using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;


namespace INFOIBV
{
    class ColorHistogram
    {

        public int[] HistogramR = new int[256];
        public int[] HistogramG = new int[256];
        public int[] HistogramB = new int[256];
        public int[] CummulativeHistogramR = new int[256];
        public int[] CummulativeHistogramG = new int[256];
        public int[] CummulativeHistogramB = new int[256];
        public int ArLow = 0;
        public int ArHigh = 0;
        public int AgLow = 0;
        public int AgHigh = 0;
        public int AbLow = 0;
        public int AbHigh = 0;

        public ColorHistogram(Color[,] Image)
        {
            ComputeColorHistogram(Image);
            ComputeLowAndHigh();
            ComputeCummulativeHistogram(Image);
        }


        public void ComputeColorHistogram(Color[,] Image)
        {
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    Color pixelColor = Image[x, y];
                    HistogramR[pixelColor.R]++;
                    HistogramG[pixelColor.G]++;
                    HistogramB[pixelColor.B]++;
                }
            }

        }

        public void ComputeCummulativeHistogram(Color[,] Image) {
            for (int i= 0; i<256; i++) {
                if (i == 0)
                {
                    CummulativeHistogramR[i] = HistogramR[i];
                    CummulativeHistogramG[i] = HistogramG[i];
                    CummulativeHistogramB[i] = HistogramB[i];
                }
                else {
                    CummulativeHistogramR[i] = HistogramR[i] + CummulativeHistogramR[i-1];
                    CummulativeHistogramG[i] = HistogramG[i] + CummulativeHistogramG[i-1];
                    CummulativeHistogramB[i] = HistogramB[i] + CummulativeHistogramB[i-1];
                }
            }
        }

        public void ComputeLowAndHigh()
        {
            //Lows calculation
            bool foundrlow = false;
            bool foundglow = false;
            bool foundblow = false;

            for (int i = 0; i < 256; i++)
            {
                if (!foundrlow)
                {
                    if (HistogramR[i] != 0) { ArLow = i; foundrlow = true; }
                }
                if (!foundglow)
                {
                    if (HistogramG[i] != 0) { AgLow = i; foundglow = true; }
                }
                if (!foundblow)
                {
                    if (HistogramB[i] != 0) { AbLow = i; foundblow = true; }
                }
            }

            //Highs calculation
            bool foundrhigh = false;
            bool foundghigh = false;
            bool foundbhigh = false;

            for (int i = 255; i >= 0; i--)
            {
                if (!foundrhigh)
                {
                    if (HistogramR[i] != 0) { ArHigh = i; foundrhigh = true; }
                }
                if (!foundghigh)
                {
                    if (HistogramG[i] != 0) { AgHigh = i; foundghigh = true; }
                }
                if (!foundbhigh)
                {
                    if (HistogramB[i] != 0) { AbHigh = i; foundbhigh = true; }
                }
            }

        }


    }
}
