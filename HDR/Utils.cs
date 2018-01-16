using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HDR
{
    public class Utils
    {
        public static double[] runit(double[][] A, double[] b)
        {
            File.WriteAllText("ASize.txt", A.Length.ToString() + "\n" + A[0].Length.ToString());
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < A.Length; i++)
                for (int j = 0; j < A[i].Length; j++)
                {
                    stringBuilder.Append(A[i][j].ToString());
                    if (j + 1 < b.Length)
                        stringBuilder.Append("\n");
                }
            File.WriteAllText("A.txt", stringBuilder.ToString());

            File.WriteAllText("bSize.txt", b.Length.ToString());
            StringBuilder stringBuilderb = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                stringBuilderb.Append(b[i].ToString());
                if (i + 1 < b.Length)
                    stringBuilderb.Append("\n");
            }
            File.WriteAllText("b.txt", stringBuilderb.ToString());

            double[] x = new double[256];

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "main.py",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    result = result.Replace("[", "");
                    result = result.Replace("]", "");
                    result = result.Replace(".", ",");
                    result = result.Replace("\n", "");
                    string[] splittedResult = result.Split('\r');
                    for (int i = 0; i < 256; i++)
                        x[i] = Double.Parse(splittedResult[i]);
                }
            }
            return x;
        }

        /// <summary>
        /// Get bitmapSource from bitmap
        /// </summary>
        /// <param name="bmp">bitmap</param>
        /// <returns>bitmapSource</returns>
        public static BitmapSource getSource(Bitmap bmp)
        {
            BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(
                    bmp.Width,
                    bmp.Height
                    ));
            bmp.Dispose();
            return bmpSource;
        }

        /// <summary>
        /// Create image view from bitmap
        /// </summary>
        /// <param name="bmp">bitmap to be displayed</param>
        /// <param name="width">width of image view</param>
        /// <param name="height">height of image view</param>
        /// <returns>Image view control</returns>
        public static System.Windows.Controls.Image GetImageView(Bitmap bmp)
        {
            return new System.Windows.Controls.Image
            {
                Width = bmp.Width,
                Height = bmp.Height,
                Source = getSource(bmp),
                Stretch = Stretch.Fill
            };
        }
    }
}
