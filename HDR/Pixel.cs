namespace HDR
{
    public class Pixel
    {
        public byte RED { get; set; }
        public byte GREEN { get; set; }
        public byte BLUE { get; set; }

        public Pixel(byte r, byte g, byte b)
        {
            RED = r;
            GREEN = g;
            BLUE = b;
        }

        internal int getChannel(int ch)
        {
            switch (ch)
            {
                case 0:
                    return BLUE;
                case 1:
                    return GREEN;
                case 2:
                    return RED;
                default:
                    return 0;
            }
        }
    }
}
