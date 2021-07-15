using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using QRCoder;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.Pages.ReceivePages;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Receive.xaml
    /// </summary>
    public partial class Receive : Window
    {

        string baseURL = URLConfiguration.BaseURL; //  "http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();

        private string walletName;

        public string WalletName
        {
            get
            {
                return this.walletName;
            }
            set
            {
                this.walletName = value;
            }
        }

        public Receive()
        {
            InitializeComponent();
            
        }

        public Receive(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
           
        }

        private void restoreButton_Click(object sender, RoutedEventArgs e)
        {           
            this.Close();
        }

        private void XelsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveContent.Content = new XelsPage(this.walletName);
            this.XelsButton.Focus();
            //Receive r = new Receive(this.walletName);
            //r.Show();
            //this.Close();
        }

        private void selsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveContent.Content = new SelsPage(this.walletName);   
            //ReceiveSelsBels rsb = new ReceiveSelsBels(this.walletName);
            //rsb.Show();
            //this.Close();
        }

        private void BelsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveContent.Content = new BelsPage(this.walletName);

            //ReceiveSelsBels rsb = new ReceiveSelsBels(this.walletName);
            //rsb.Show();
            //this.Close();
        }

    }
}
