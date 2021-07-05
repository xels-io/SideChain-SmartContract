namespace XelsDesktopWalletApp.Views
{
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using QRCoder;

    /// <summary>
    /// Interaction logic for Detail.xaml
    /// </summary>
    public partial class Detail : Window
    {
        public Detail()
        {
            InitializeComponent();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            //Restore restore = new Restore();
            //restore.Show();
            //this.Close();
            MyPopup.IsOpen = true;
        }
        private void restoreButton_Click(object sender, RoutedEventArgs e)
        {
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            //QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(this.textBoxTextToQr.Text, QRCodeGenerator.ECCLevel.H);
            QRCode qrCode = new QRCode(qRCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            this.image.Source = BitmapToImageSource(qrCodeImage);
        }

        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.MyPopup.IsOpen = false;

        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            this.MyPopup.IsOpen = false;
        }

        private void Hyperlink_RequestNavigate(object sender)
        {
            Login lg = new Login();
            lg.Show();
            this.Close();
        }
    }
}
