
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace HDR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyImageDouble HDRImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonHDR_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            labelOutput.Content = "";

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif) | *.jpg; *.jpeg; *.jpe; *.jfif",
                Title = "Please select image",
                Multiselect = true
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Load images
                MyImage[] images = loadImages(dialog.FileNames);

                // Get parameters
                int smoothfactor = (int)getParamater(textboxSmoothFactor);
                int samples = (int)getParamater(textboxSample);

                // Display images
                drawImages(images);

                // Process images
                HDResult hdrResult = ImageProcessing.HDR(images, smoothfactor, samples);

                // Draw response graphs
                drawReponsesGraph(hdrResult.response);

                // Save HDR image
                HDRImage = hdrResult.HDR;

                MyImage tempImage = HDRImage.ToMyImage();

                // Show HDR image
                processedImage.Source = Utils.getSource(tempImage.GetBitmap());

                // Create histograms
                displayHistograms(tempImage);
            }

            watch.Stop();
            string timeTaken = String.Format(" HDR {0} ms", watch.ElapsedMilliseconds.ToString());
            Console.WriteLine(timeTaken);
            labelOutput.Content += timeTaken;
        }

        private void buttonCLAHE_Click(object sender, RoutedEventArgs e)
        {
            if (HDRImage == null)
                System.Windows.MessageBox.Show("No HDR image generated", "Error", MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

            // Get parameters
            int windowSize = (int)getParamater(textboxAHEWindowSize);
            double contrastLimit = getParamater(textboxClipLimit);

            // Convert to MyImage
            MyImage myImage = HDRImage.ToMyImage();

            // Calculate each channel separated

            // Do paralel image process on each channel 
            Parallel.ForEach(myImage.bitplane, (bitplane, state, ch) =>
            {
                // Process current channel
                ImageProcessing.CLAHE(ref bitplane, windowSize, contrastLimit);
            });
            
            // Create histograms
            displayHistograms(myImage);

            // Draw image on screen
            processedImage.Source = Utils.getSource(myImage.GetBitmap());
        }

        private MyImage[] loadImages(string[] fileNames)
        {
            List<MyImage> images = new List<MyImage>();
            foreach (string filename in fileNames)
                using (Bitmap bmp = new Bitmap(filename))
                    images.Add(new MyImage(bmp));
            labelOutput.Content += String.Format("Width: {0} Height: {0}", images[0].width, images[0].height);

            // Sort image by exposure
            images = images.OrderByDescending(o => o.exposureTime).ToList();

            return images.ToArray();
        }

        private void drawImages(MyImage[] images)
        {
            stackImages.Children.Clear();
            foreach (MyImage image in images)
            {
                // Print image details
                image.print();

                // Add image to stack
                System.Windows.Controls.Label label = new System.Windows.Controls.Label
                {
                    Content = String.Format("Exposure time {0} sec", image.exposureTime.ToString())
                };
                stackImages.Children.Add(label);
                stackImages.Children.Add(Utils.GetImageView(image.GetBitmap(200, 200)));
            }
        }

        private void drawReponsesGraph(double[][] responses)
        {
            int[] y = new int[256];
            for (int i = 0; i < 256; i++)
                y[i] = i;

            channelR.Plot(responses[0], y); // Red
            channelG.Plot(responses[1], y); // Green
            channelB.Plot(responses[2], y); // Blue
        }

        private void drawHistogram(double[][] responses)
        {
            int[] y = new int[256];
            for (int i = 0; i < 256; i++)
                y[i] = i;

            channelR.Plot(responses[0], y); // Red
            channelG.Plot(responses[1], y); // Green
            channelB.Plot(responses[2], y); // Blue
        }

        private void displayHistograms(MyImage myImage)
        {
            int[] y = new int[256];
            for (int i = 0; i < 256; i++)
                y[i] = i;
            
            // Calculate each channel separated
            double[][] histograms = new double[myImage.numCh][];
            double[][] comulativeFrequencies = new double[myImage.numCh][];

            // Do paralel image process on each channel 
            Parallel.ForEach(myImage.bitplane, (bitplane, state, ch) =>
            {
                // Calculate Histogram
                histograms[ch] = ImageProcessing.calculateHistogram(bitplane);

                // Calculate Comulative Histogram
                comulativeFrequencies[ch] = ImageProcessing.calculateComulativeFrequency(histograms[ch]);
            });

            // Draw graphs
            histogramR.PlotBars(histograms[2]);
            histogramG.PlotBars(histograms[1]);
            histogramB.PlotBars(histograms[0]);

            comulativeHistogramR.PlotY(comulativeFrequencies[2]);
            comulativeHistogramG.PlotY(comulativeFrequencies[1]);
            comulativeHistogramB.PlotY(comulativeFrequencies[0]);
        }

        private double getParamater(System.Windows.Controls.TextBox textbox)
        {
            return Double.Parse(textbox.Text);
        }
    }
}
