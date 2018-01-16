namespace HDR
{
    public class MyBitplane
    {
        public int width { get; set; }
        public int height { get; set; }

        public byte[,] data { get; set; }
        
        public MyBitplane(int w, int h)
        {
            width = w;
            height = h;

            data = new byte[height, width];
        }

        public byte GetPixel(int x, int y)
        {
            return data[y, x];
        }

        public void SetPixel(int x, int y, byte value)
        {
            data[y, x] = value;
        }
    }
}
