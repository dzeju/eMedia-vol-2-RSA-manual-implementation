using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace e_media0_3
{
    /// <summary>
    /// Interaction logic for PictureWindow.xaml
    /// </summary>
    public partial class PictureWindow : Window
    {
        public PictureWindow(string file)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(file))
            {
                MessageBox.Show("Did not specify file, nerd");
                this.Close();
            }
            else
            {
                try
                {
                    Bitmap myBmp = new Bitmap(file);
                    imgDisp.Source = BitmapToImageSource(myBmp);
                    imgDisp.Height = myBmp.Height;
                    imgDisp.Width = myBmp.Width;
                    myBmp.Dispose();
                }
                catch
                {
                    MessageBox.Show("Wrong file, nerd");
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Converts bitmap to image source for WPF to display
        /// </summary>
        /// <param name="bitmap">Bitmap to convert.</param>
        /// <returns>Bitmap image for source.</returns>
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}

