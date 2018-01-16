namespace HDR
{
    public class Pixel
    {
        public byte RED { get; set; }
        public byte GREEN { get; set; }
        public byte BLUE { get; set; }

        public Pixel(byte rED, byte gREEN, byte bLUE)
        {
            RED = rED;
            GREEN = gREEN;
            BLUE = bLUE;
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
