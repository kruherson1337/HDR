using CenterSpace.NMath.Core;
using System;
using System.Threading.Tasks;

namespace HDR
{
    class ImageProcessing
    {
        internal static HDResult HDR(MyImage[] images, int smoothfactor, int samples)
        {
            // Prepare data
            int imagesCount = images.Length;
            int width = images[0].width;
            int height = images[0].height;
            int numCh = images[0].numCh;

            // Create random points for samples
            Random random = new Random();
            int[] randomX = new int[samples];
            int[] randomY = new int[samples];
            for (int i = 0; i < samples; ++i)
            {
                randomX[i] = random.Next(width);
                randomY[i] = random.Next(height);
            }

            // Get sample pixels and exposure time
            Pixel[][] imagesSamples = new Pixel[imagesCount][];
            double[] exposureTime = new double[imagesCount];
            for (int i = 0; i < imagesCount; ++i)
            {
                imagesSamples[i] = images[i].getSamples(samples, randomX, randomY);
                exposureTime[i] = images[i].exposureTime;
            }

            // Process each channel
            HDResult hdrResult = new HDResult(new MyImageDouble(width, height, numCh));
            Parallel.For(0, numCh, ch =>
            {
                hdrResult.response[ch] = GSolve(imagesSamples, exposureTime, smoothfactor, ch);
                hdrResult.HDR.bitplane[ch] = RadianceMap(getChannelFromimages(images, ch), hdrResult.response[ch], width, height, numCh);
            });

            hdrResult.HDR.log();
            hdrResult.HDR.normalize();
            hdrResult.HDR.divide(hdrResult.HDR.findMax());
            hdrResult.HDR.multiply(255.0);

            return hdrResult;
        }

        private static MyBitplaneDouble RadianceMap(MyImageDouble[] imagesChannels, double[] response, int width, int height, int numCh)
        {
            MyBitplaneDouble radianceMap = new MyBitplaneDouble(width, height);
            int imageSize = imagesChannels.Length;

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    // Prepare data
                    double[] g = new double[imageSize];
                    double[] w = new double[imageSize];

                    for (int i = 0; i < imageSize; ++i)
                    {
                        g[i] = response[(int)imagesChannels[i].bitplane[0].GetPixel(x, y)];
                        w[i] = W(imagesChannels[i].bitplane[0].GetPixel(x, y));
                    }

                    double sumW = sum(w);
                    double[] logTimes = logExpoTimes(imagesChannels);

                    if (sumW > 0)
                        radianceMap.SetPixel(x, y, sumMatrix(multiply(divide(minus(g, logTimes), sumW), w)));
                    else
                    {
                        int middle = imageSize / 2;
                        radianceMap.SetPixel(x, y, g[middle] - logTimes[middle]);
                    }
                }

            radianceMap.exp();

            return radianceMap;
        }

        private static double[] GSolve(Pixel[][] imagesSamples, double[] exposureTime, int smoothFactor, int ch)
        {
            int z_max = 256;
            int p = imagesSamples.Length;
            int n = imagesSamples[0].Length;

            double[,] A = new double[n * p + z_max + 1, z_max + n];
            double[] b = new double[A.GetLength(0)];

            int k = 0;
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < p; ++j)
                {
                    int z = imagesSamples[j][i].getChannel(ch);
                    double wij = W(z);
                    A[k, z] = wij;
                    A[k, z_max + i] = -wij;
                    b[k] = wij * Math.Log(exposureTime[j]);
                    k += 1;
                }

            // Limit middle value
            A[k, 128] = 1;
            k += 1;

            // Smoothing
            for (int i = 0; i < z_max - 1; ++i)
            {
                double w_k = W(i + 1);
                A[k, i] = smoothFactor * W(w_k);
                A[k, i + 1] = -2 * smoothFactor * w_k;
                A[k, i + 2] = smoothFactor * w_k;
                k += 1;
            }

            // Solve SVD
            var AA = new DoubleMatrix(A);
            var bb = new DoubleVector(b);
            var lsq = new DoubleLeastSquares(AA, bb, true);

            double[] g = new double[256];
            for (int i = 0; i < 256; ++i)
                g[i] = lsq.X[i];

            return g;
        }

        private static double W(double value)
        {
            double z_min = 0;
            double z_max = 255;
            if (value <= (z_min + z_max) / 2)
                return value - z_min;
            return z_max - value;
        }

        private static MyImageDouble[] getChannelFromimages(MyImage[] images, int ch)
        {
            int imagesCount = images.Length;
            MyImageDouble[] imagesChannels = new MyImageDouble[imagesCount];
            for (int i = 0; i < imagesCount; ++i)
                imagesChannels[i] = new MyImageDouble(images[i].bitplane[ch], images[i].exposureTime);
            return imagesChannels;
        }

        private static double sum(double[] x)
        {
            double sumW = 0;
            foreach (double ww in x)
                sumW += ww;
            return sumW;
        }

        private static double sumMatrix(double[,] x)
        {
            int size = x.GetLength(0);
            double sum = 0;
            for (int xx = 0; xx < size; ++xx)
                for (int yy = 0; yy < size; ++yy)
                    sum += x[xx, yy];
            return sum;
        }

        private static double[,] multiply(double[,] x1, double[] x2)
        {
            int size = x1.GetLength(0);
            double[,] result = new double[size, size];
            for (int xx = 0; xx < size; ++xx)
                for (int yy = 0; yy < size; ++yy)
                    result[xx, yy] = x1[xx, yy] * x2[yy];
            return result;
        }

        private static double[,] divide(double[,] x1, double divider)
        {
            int size = x1.GetLength(0);
            double[,] result = new double[size, size];
            for (int xx = 0; xx < size; ++xx)
                for (int yy = 0; yy < size; ++yy)
                    result[xx, yy] = x1[xx, yy] / divider;
            return result;
        }

        private static double[,] minus(double[] x1, double[] x2)
        {
            int size = x1.Length;
            double[,] result = new double[size, size];
            for (int xx = 0; xx < size; ++xx)
                for (int yy = 0; yy < size; ++yy)
                    result[xx, yy] = x1[yy] - x2[yy];
            return result;
        }

        private static double[] logExpoTimes(MyImageDouble[] imagesChannels)
        {
            double[] logTimes = new double[imagesChannels.Length];
            for (int i = 0; i < logTimes.Length; ++i)
                logTimes[i] = Math.Log(imagesChannels[i].exposureTime);
            return logTimes;
        }
    }
}
