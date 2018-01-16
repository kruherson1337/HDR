using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using CenterSpace.NMath.Core;

namespace HDR
{
    public class Utils
    {
        public static double[] solveSVD(double[][] A, double[] b)
        {


           // CultureInfo original = Thread.CurrentThread.CurrentCulture;

            // This example uses strings representing numbers in the US locale
            // so change the current culture info.  For example, "0.446"
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Calculate the slope and intercept of the linear least squares fit
            // through the five points:
            // (20, .446) (30, .601), (40, .786), (50, .928), (60, .950)
            double[,] AMod = new double[A.Length,A[0].Length];
            for (int ii = 0; ii < A.Length; ii++)
            {
                for (int jj = 0; jj < A[ii].Length; jj++)
                {
                    AMod[ii, jj] = A[ii][jj];
                }
            }
            var AA = new DoubleMatrix(AMod);
            var bb = new DoubleVector(b);

            // Back to your original culture.
            //Thread.CurrentThread.CurrentCulture = original;

            // We want our straight line to be of the form y = mx + b, where b is
            // not necessarily equal to zero. Thus we will set the third 
            // constructor argument to true so that we calculate the intercept
            // parameter. 
            var lsq = new DoubleLeastSquares(AA, bb, true);

            //Console.WriteLine("b = {0:F4}", lsq.X);
            //Console.WriteLine();

            //Console.WriteLine("Y-intercept = {0}", lsq.X[0]);
            //Console.WriteLine("Slope = {0}", lsq.X[1]);

            // We can look at the residuals which are the difference between the 
            // actual value of y at a point x, and the corresponding point y on 
            // line for the same x.
            //Console.WriteLine("Residuals = {0}", lsq.Residuals.ToString("F3"));

            // Finally, we can look at the residual sum of squares, which is the
            // sum of the squares of the elements in the residual vector.
            //Console.WriteLine("Residual Sum of Squares (RSS) = {0}", lsq.ResidualSumOfSquares.ToString("F3"));

            // The least squares class can also be used to solve "rank-deficient" least 
            // square problems:
            //A = new DoubleMatrix("6x4 [0 9 -6 3  -3 0 -3 0  1 3 -1 1  1 3 -1 1  -2 0 -2 0  3 6 -1 2]");
            //AA = new DoubleVector("[-3 5 -2 2 1 -2]");

            // For this problem we will specify a tolerance for computing the effective rank
            // of the matrix A, and we will not have the class add an intercept parameter
            // for us.
            //lsq = new DoubleLeastSquares(A, AA, 1e-10);
            //Console.WriteLine("Least squares solution = {0}", lsq.X.ToString("F3"));
            //Console.WriteLine("Rank computed using a tolerance of {0}, = {1}",
            //  lsq.Tolerance, lsq.Rank);

            // You can even use the least squares class to solve under-determined systems
            // (the case where A has more columns than rows).
            //A = new DoubleMatrix("6x4 [-3 -1 6 -5  5 4 -6 8  7 5 0 -4  -7 4 0 3  -7 7 -8 2  3 4 2 -4]");
            //AA = new DoubleVector("[-3 1 8 -2]");
            //lsq = new DoubleLeastSquares(A.Transpose(), AA, 1e-8);
            //Console.WriteLine("Solution to under-determined system = {0}", lsq.X.ToString("F3"));
            // Console.WriteLine("Rank computed using a tolerance of {0}, = {1}",
            //    lsq.Tolerance, lsq.Rank);

            //  Console.WriteLine();
            // Console.WriteLine("Press Enter Key");
            // Console.Read();

            return lsq.X.ToArray();
          
            // return runit(A, b);
        }

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

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python";
            start.Arguments = "main.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
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
        public static System.Windows.Controls.Image GetImageView(Bitmap bmp, double width, double height)
        {
            return new System.Windows.Controls.Image
            {
                Source = getSource(bmp),
                Width = width,
                Height = height,
                Stretch = Stretch.Fill
            };
        }
    }
}
