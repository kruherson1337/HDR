using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HDR
{
    public class MyImage
    {
        public int width { get; set; }
        public int height { get; set; }
        public int numCh { get; set; }
        public double exposureTime { get; set; }
        public List<MyBitplane> bitplane { get; set; }

        public MyImage(Bitmap bmp)
        {
            width = bmp.Width;
            height = bmp.Height;
            exposureTime = getExposureTime(bmp);

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                  ImageLockMode.ReadOnly, bmp.PixelFormat);

            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed: numCh = 1; break;
                case PixelFormat.Format16bppGrayScale: numCh = 2; break;
                case PixelFormat.Format24bppRgb: numCh = 3; break;
                case PixelFormat.Format32bppArgb: numCh = 4; break;
                default: numCh = 1; break;
            }

            byte[] pixels = new byte[bmp.Width * bmp.Height * numCh];
            Marshal.Copy(bd.Scan0, pixels, 0, pixels.Length);
            bmp.UnlockBits(bd);

            bitplane = new List<MyBitplane>();
            for (int i = 0; i < numCh; i++)
                bitplane.Add(new MyBitplane(width, height));

            int pos = 0;
            for (int j = 0; j < height; ++j)
                for (int i = 0; i < width; ++i)
                    for (int ch = 0; ch < numCh; ++ch)
                        bitplane[ch].SetPixel(i, j, pixels[pos++]);


            bmp.Dispose();
        }

        public MyImage(int w, int h, int ch, double expoTime)
        {
            numCh = ch;
            width = w;
            height = h;
            exposureTime = expoTime;

            bitplane = new List<MyBitplane>();
            for (int i = 0; i < numCh; ++i)
                bitplane.Add(new MyBitplane(width, height));
        }

        public Bitmap GetBitmap()
        {
            Bitmap bmp;
            switch (numCh)
            {
                case 1: bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed); break;
                case 2: bmp = new Bitmap(width, height, PixelFormat.Format16bppGrayScale); break;
                case 3: bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb); break;
                case 4: bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb); break;
                default: bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed); break;
            }
            byte[] pixels = new byte[width * height * numCh];

            int pos = 0;
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                    for (int ch = 0; ch < numCh; ++ch)
                        pixels[pos++] = (byte)bitplane[ch].GetPixel(x, y);


            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            Marshal.Copy(pixels, 0, bd.Scan0, pixels.Length);

            bmp.UnlockBits(bd);

            return bmp;
        }

        public Bitmap GetBitmap(int newWidth, int newHeight)
        {
            return new Bitmap(GetBitmap(), new Size(newWidth, newHeight));
        }  
        
        public Pixel[] getSamples(int count, int[] randomX, int[] randomY)
        {
            Pixel[] sample = new Pixel[count];            
            for (int j = 0; j < count; ++j)
            {
                byte R = bitplane[2].GetPixel(randomX[j], randomY[j]);
                byte G = bitplane[1].GetPixel(randomX[j], randomY[j]);
                byte B = bitplane[0].GetPixel(randomX[j], randomY[j]);

                sample[j] = new Pixel(B, G, R);
            }
            return sample;
        }

        private double getExposureTime(Bitmap bmp)
        {
            double exposure = 0.0;
            foreach (PropertyItem propertyItem in bmp.PropertyItems)
            {
                if (propertyItem.Id == 33434)
                {
                    int a = BitConverter.ToInt32(propertyItem.Value, 0);
                    int b = BitConverter.ToInt32(propertyItem.Value, 4);
                    exposure = (double)a / b;
                }
            }
            return exposure;
        }

        public void print()
        {
            Console.WriteLine("Width: " + width);
            Console.WriteLine("Height: " + height);
            Console.WriteLine("Exposure: " + exposureTime.ToString());
        }
    }
}
