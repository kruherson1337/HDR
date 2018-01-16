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
            Random random = new Random(42);
            int width = images[0].Width;
            int height = images[0].Height;
            int numCh = images[0].NumCh;
            HDResult hdrResult = new HDResult(new MyImageDouble(width, height, numCh));
            int[] randomX = new int[samples];
            int[] randomY = new int[samples];
            Pixel[][] imagesSamples = new Pixel[images.Count][];
            double[] exposureTime = new double[images.Count];

            // Create random points for samples
            for (int i = 0; i < samples; i++)
            {
                randomX[i] = random.Next(width);
                randomY[i] = random.Next(height);
            }

            // Get sample pixels and exposure time
            for (int i = 0; i < images.Count; i++)
            {
                imagesSamples[i] = images[i].getSamples(samples, randomX, randomY);
                exposureTime[i] = images[i].ExposureTime;
            }

            // Process each channel
            Parallel.For(0, numCh, ch =>
            {
                MyImageDouble[] imagesChannels = new MyImageDouble[images.Count];
                for (int i = 0; i < images.Count; i++)
                    imagesChannels[i] = new MyImageDouble(images[i].Bitplane[ch], images[i].ExposureTime);
                hdrResult.response[ch] = GSolve(imagesSamples, exposureTime, smoothfactor, ch);
                hdrResult.HDR.Bitplane[ch] = RadianceMap(imagesChannels, hdrResult.response[ch], width, height, numCh);
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
                    double sumW = 0;
                    double[] logTimes = new double[imagesChannels.Length];

                    for (int i = 0; i < imagesChannels.Length; i++)
                    {
                        g.Add(response[(int)imagesChannels[i].Bitplane[0].GetPixel(x, y)]);
                        w.Add(W(imagesChannels[i].Bitplane[0].GetPixel(x, y)));
                    }

                    foreach (double ww in w)
                        sumW += ww;

                    for (int i = 0; i < logTimes.Length; i++)
                        logTimes[i] = Math.Log(imagesChannels[i].ExposureTime);

                    if (sumW > 0)
                    {
                        int size = logTimes.Length;
                        double[][] ggMinLogTimes = new double[size][];
                        for (int xx = 0; xx < size; xx++)
                        {
                            ggMinLogTimes[xx] = new double[size];
                            for (int yy = 0; yy < size; yy++)
                            {
                                ggMinLogTimes[xx][yy] = g[yy] - logTimes[yy];
                            }
                        }

                        double[][] ggDivideSum = new double[size][];
                        for (int xx = 0; xx < size; xx++)
                        {
                            ggDivideSum[xx] = new double[size];
                            for (int yy = 0; yy < size; yy++)
                            {
                                ggDivideSum[xx][yy] = ggMinLogTimes[xx][yy] / sumW;
                            }
                        }

                        double[][] ww = new double[size][];
                        for (int xx = 0; xx < size; xx++)
                        {
                            ww[xx] = new double[size];
                            for (int yy = 0; yy < size; yy++)
                            {
                                ww[xx][yy] = ggDivideSum[xx][yy] * w[yy];
                            }
                        }

                        double sum = 0;
                        for (int xx = 0; xx < size; xx++)
                            for (int yy = 0; yy < size; yy++)
                                sum += ww[xx][yy];

                        radianceMap.SetPixel(x, y, sum);
                    }
                    else
                    {
                        int middle = imagesChannels.Length / 2;
                        radianceMap.SetPixel(x, y, g[middle] - Math.Log(imagesChannels[middle].ExposureTime));
                    }
                }


            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                    radianceMap.SetPixel(x, y, Math.Exp(radianceMap.GetPixel(x, y)));

            return radianceMap;
        }

        private static double[] GSolve(Pixel[][] imagesSamples, double[] exposureTime, int smoothFactor, int ch)
        {
            int z_max = 256;
            int p = imagesSamples.Length;
            int n = imagesSamples[0].Length;

            double[][] A = new double[n * p + z_max + 1][];
            for (int i = 0; i < A.Length; ++i)
                A[i] = new double[z_max + n];

            double[] b = new double[A.Length];

            int k = 0;
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < p; ++j)
                {
                    int z = imagesSamples[j][i].getChannel(ch);
                    double wij = W(z);
                    A[k][z] = wij;
                    A[k][z_max + i] = -wij;
                    b[k] = wij * Math.Log(exposureTime[j]);
                    k += 1;
                }
            }

            // Limit middle value
            A[k][128] = 1;
            k += 1;

            // Smoothing
            for (int i = 0; i < z_max - 1; ++i)
            {
                double w_k = W(i + 1);
                A[k][i] = smoothFactor * W(w_k);
                A[k][i + 1] = -2 * smoothFactor * w_k;
                A[k][i + 2] = smoothFactor * w_k;
                k += 1;
            }

            // Solve SVD
            double[] x = Utils.solveSVD(A, b);

            double[] g = new double[256];
            for (int i = 0; i < 256; i++)
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
    }
}
