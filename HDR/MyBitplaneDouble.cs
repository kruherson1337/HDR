namespace HDR
{
    public class MyBitplaneDouble
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public double[,] PixelData { get; set; }
        
        public MyBitplaneDouble(MyBitplane bitplane)
        {
            this.Width = bitplane.Width;
            this.Height = bitplane.Height;

            PixelData = new double[Height, Width];
            for (int y = 0; y < this.Height; ++y)
                for (int x = 0; x < this.Width; ++x)
                    SetPixel(x, y, (double)bitplane.GetPixel(x, y));
        }

        public MyBitplaneDouble(int w, int h)
        {
            Width = w;
            Height = h;

            PixelData = new double[Height, Width];
        }
        
        public double GetPixel(int x, int y)
        {
            return PixelData[y, x];
        }

        public void SetPixel(int x, int y, double value)
        {
            PixelData[y, x] = value;
        }        
    }
}
