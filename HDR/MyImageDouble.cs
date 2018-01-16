using System;
using System.Collections.Generic;

namespace HDR
{

    public class MyImageDouble
    {
        public int width { get; set; }
        public int height { get; set; }
        public int numCh { get; set; }
        public double exposureTime { get; set; }

        public List<MyBitplaneDouble> bitplane = new List<MyBitplaneDouble>();

        public MyImageDouble(int w, int h, int cch)
        {
            numCh = cch;
            width = w;
            height = h;

            for (int ch = 0; ch < numCh; ++ch)
                bitplane.Add(new MyBitplaneDouble(width, height));
        }

        public MyImageDouble(MyBitplane myBitplane, double expoTime)
        {
            numCh = 1;
            width = myBitplane.width;
            height = myBitplane.height;
            exposureTime = expoTime;
            bitplane.Add(new MyBitplaneDouble(myBitplane));
        }

        public MyImage ToMyImage()
        {
            MyImage myImage = new MyImage(width, height, numCh, exposureTime);
            for (int ch = 0; ch < numCh; ++ch)
                myImage.bitplane[ch] = bitplane[ch].ToMyBitplane();
            return myImage;
        }

        public void log()
        {
            for (int ch = 0; ch < numCh; ++ch)
                bitplane[ch].log();
        }

        public void multiply(double value)
        {
            for (int ch = 0; ch < numCh; ++ch)
                bitplane[ch].multiply(value);
        }

        public void divide(double value)
        {
            for (int ch = 0; ch < numCh; ++ch)
                bitplane[ch].divide(value);
        }

        public void normalize()
        {
            double min = findMin();
            double max = findMax();
            for (int ch = 0; ch < numCh; ++ch)
                bitplane[ch].normalize(min, max);
        }

        public double findMin()
        {
            double min = Double.MaxValue;
            for (int ch = 0; ch < numCh; ++ch)
                if (min > bitplane[ch].findMin())
                    min = bitplane[ch].findMin();
            return min;
        }

        public double findMax()
        {
            double max = Double.MinValue;
            for (int ch = 0; ch < numCh; ++ch)
                if (max < bitplane[ch].findMax())
                    max = bitplane[ch].findMax();
            return max;
        }
    }
}
