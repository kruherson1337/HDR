using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDR
{
    class ImageProcessing
    {
        internal static HDResult HDR(List<MyImage> images, int smoothfactor, int samples)
        {
            // Prepare data
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
            Pixel[][] imagesSamples = new Pixel[images.Count][];
            double[] exposureTime = new double[images.Count];
            for (int i = 0; i < images.Count; ++i)
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

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    // Prepare data
                    List<double> g = new List<double>();
                    List<double> w = new List<double>();

                    for (int i = 0; i < imagesChannels.Length; ++i)
                    {
                        g.Add(response[(int)imagesChannels[i].bitplane[0].GetPixel(x, y)]);
                        w.Add(W(imagesChannels[i].bitplane[0].GetPixel(x, y)));
                    }

                    double sumW = sum(w);
                    double[] logTimes = logExpoTimes(imagesChannels);

                    if (sumW > 0)
                    {
                        int size = logTimes.Length;
                        radianceMap.SetPixel(x, y, sumMatrix(size, multiply(w, size, divide(sumW, size, minus(g, logTimes, size)))));
                    }
                    else
                    {
                        int middle = imagesChannels.Length / 2;
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
            {
                for (int j = 0; j < p; ++j)
                {
                    int z = imagesSamples[j][i].getChannel(ch);
                    double wij = W(z);
                    A[k, z] = wij;
                    A[k, z_max + i] = -wij;
                    b[k] = wij * Math.Log(exposureTime[j]);
                    k += 1;
                }
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
            double[] x = Utils.solveSVD(A, b);

            double[] g = new double[256];
            for (int i = 0; i < 256; ++i)
                g[i] = x[i];

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

        private static MyImageDouble[] getChannelFromimages(List<MyImage> images, int ch)
        {
            MyImageDouble[] imagesChannels = new MyImageDouble[images.Count];
            for (int i = 0; i < images.Count; ++i)
                imagesChannels[i] = new MyImageDouble(images[i].bitplane[ch], images[i].exposureTime);
            return imagesChannels;
        }

        private static double sum(List<double> w)
        {
            double sumW = 0;
            foreach (double ww in w)
                sumW += ww;
            return sumW;
        }

        private static double sumMatrix(int size, double[][] ww)
        {
            double sum = 0;
            for (int xx = 0; xx < size; ++xx)
                for (int yy = 0; yy < size; ++yy)
                    sum += ww[xx][yy];
            return sum;
        }

        private static double[][] multiply(List<double> w, int size, double[][] ggDivideSum)
        {
            double[][] ww = new double[size][];
            for (int xx = 0; xx < size; ++xx)
            {
                ww[xx] = new double[size];
                for (int yy = 0; yy < size; ++yy)
                {
                    ww[xx][yy] = ggDivideSum[xx][yy] * w[yy];
                }
            }

            return ww;
        }

        private static double[][] divide(double sumW, int size, double[][] ggMinLogTimes)
        {
            double[][] ggDivideSum = new double[size][];
            for (int xx = 0; xx < size; ++xx)
            {
                ggDivideSum[xx] = new double[size];
                for (int yy = 0; yy < size; ++yy)
                {
                    ggDivideSum[xx][yy] = ggMinLogTimes[xx][yy] / sumW;
                }
            }

            return ggDivideSum;
        }

        private static double[][] minus(List<double> g, double[] logTimes, int size)
        {
            double[][] ggMinLogTimes = new double[size][];
            for (int xx = 0; xx < size; ++xx)
            {
                ggMinLogTimes[xx] = new double[size];
                for (int yy = 0; yy < size; ++yy)
                {
                    ggMinLogTimes[xx][yy] = g[yy] - logTimes[yy];
                }
            }

            return ggMinLogTimes;
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
