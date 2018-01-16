
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            labelOutput.Content = "";

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif) | *.jpg; *.jpeg; *.jpe; *.jfif",
                Title = "Prosim izberite sliko",
                Multiselect = true
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<MyImage> images = new List<MyImage>();
                foreach (string filename in dialog.FileNames)
                    using (Bitmap bmp = new Bitmap(filename))
                        images.Add(new MyImage(bmp));

                // Output width and height
                labelOutput.Content += String.Format("Width: {0} Height: {0}", images[0].Width, images[0].Height);

                // Sort image by exposure
                images = images.OrderByDescending(o => o.ExposureTime).ToList();

                // Display images
                stackImages.Children.Clear();
                foreach (MyImage image in images)
                {
                    // Add image to stack
                    System.Windows.Controls.Label label = new System.Windows.Controls.Label();
                    label.Content = image.ExposureTime.ToString();
                    stackImages.Children.Add(label);
                    stackImages.Children.Add(Utils.GetImageView(image.GetBitmap(200, 200), 200, 200));
                }

                int smoothfactor = Int32.Parse(textboxSmoothFactor.Text);
                int samples = Int32.Parse(textboxSample.Text);

                HDResult hdrResult = ImageProcessing.HDR(images, smoothfactor, samples);

                drawGraph(hdrResult.response[0]);

                hdrImage.Source = Utils.getSource(hdrResult.HDR.ToMyImage().GetBitmap());
            }

            watch.Stop();
            string timeTaken = String.Format(" HDR {0} ms", watch.ElapsedMilliseconds.ToString());
            Console.WriteLine(timeTaken);
            labelOutput.Content += timeTaken;

        }
    }
}
