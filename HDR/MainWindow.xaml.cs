
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace HDR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

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
                int smoothfactor = getParamater(textboxSmoothFactor);
                int samples = getParamater(textboxSample);

                // Display images
                drawImages(images);

                // Process images
                HDResult hdrResult = ImageProcessing.HDR(images, smoothfactor, samples);

                // Draw response graphs
                drawGraph(hdrResult.response[0]);

                // Show HDR image
                hdrImage.Source = Utils.getSource(hdrResult.HDR.ToMyImage().GetBitmap());
            }

            watch.Stop();
            string timeTaken = String.Format(" HDR {0} ms", watch.ElapsedMilliseconds.ToString());
            Console.WriteLine(timeTaken);
            labelOutput.Content += timeTaken;
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

        private void drawGraph(double[] values)
        {
            string[] labels = new string[256];
            for (int i = 0; i < 256; i++)
                labels[i] = i.ToString();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<double> (values),
                    PointGeometry = null
                }
            };

            Labels = labels;
            YFormatter = value => value.ToString();

            DataContext = this;
        }

        private int getParamater(System.Windows.Controls.TextBox textbox)
        {
            return Int32.Parse(textbox.Text);
        }
    }
}
