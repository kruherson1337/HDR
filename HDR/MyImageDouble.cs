using System;
using System.Collections.Generic;

namespace HDR
{

    public class MyImageDouble
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int NumCh { get; set; }
        public double ExposureTime { get; set; }
        public string ImageFileName { get; set; }

        public List<MyBitplaneDouble> Bitplane = new List<MyBitplaneDouble>();

        public MyImageDouble(int w, int h, int ch)
        {
            NumCh = ch;
            Width = w;
            Height = h;
            ImageFileName = "";

            for (int i = 0; i < NumCh; i++)
                Bitplane.Add(new MyBitplaneDouble(Width, Height));
        }

        public MyImageDouble(MyBitplane myBitplane, double expoTime)
        {
            NumCh = 1;
            Width = myBitplane.Width;
            Height = myBitplane.Height;
            ImageFileName = "";
            ExposureTime = expoTime;

            for (int i = 0; i < NumCh; i++)
                Bitplane.Add(new MyBitplaneDouble(myBitplane));
        }

        public MyImage ToMyImage()
        {
            MyImage myImage = new MyImage(Width, Height, NumCh, 0.0);
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                    {
                        myImage.Bitplane[ch].SetPixel(x, y, (byte)Math.Round(Bitplane[ch].GetPixel(x, y)));
                    }
            return myImage;
        }

        public void log()
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                        Bitplane[ch].SetPixel(x, y, Math.Log(Bitplane[ch].GetPixel(x, y)));
        }

        public void multiply(double value)
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                        Bitplane[ch].SetPixel(x, y, Bitplane[ch].GetPixel(x, y) * value);
        }

        public void divide(double value)
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                        Bitplane[ch].SetPixel(x, y, Bitplane[ch].GetPixel(x, y) / value);
        }

        public void normalize()
        {
            double min = findMin();
            double max = findMax();
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                        Bitplane[ch].SetPixel(x, y, Bitplane[ch].GetPixel(x, y) - min / max - min);
        }

        public double findMin()
        {
            double min = Double.MaxValue;
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                        if (min > Bitplane[ch].GetPixel(x, y))
                            min = Bitplane[ch].GetPixel(x, y);
            return min;
        }

        public double findMax()
        {
            double max = Double.MinValue;
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ch++)
                        if (max < Bitplane[ch].GetPixel(x, y))
                            max = Bitplane[ch].GetPixel(x, y);
            return max;
        }

    }
}
