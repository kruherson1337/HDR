namespace HDR
{
    public class MyBitplane
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public byte[,] PixelData { get; set; }
        
        public MyBitplane(int w, int h)
        {
            Width = w;
            Height = h;

            PixelData = new byte[Height, Width];
        }

        public byte GetPixel(int x, int y)
        {
            return PixelData[y, x];
        }

        public void SetPixel(int x, int y, byte value)
        {
            PixelData[y, x] = value;
        }
    }
}
