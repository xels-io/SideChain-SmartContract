using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.layout;

namespace XelsDesktopWalletApp.Views.Pages.SendPages
{
    /// <summary>
    /// Interaction logic for BelsPage.xaml
    /// </summary>
    public partial class BelsPage : Page
    {
        private readonly string baseURL = URLConfiguration.BaseURL; // "http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();
        private TransactionSending transactionSending = new TransactionSending();

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

        public BelsPage()
        {
            InitializeComponent();
        }

        public BelsPage(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendTransactionAsync();
        }

        private async Task SendTransactionAsync()
        {
            string postUrl = this.baseURL + $"/wallet/send-transaction";

            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this.transactionSending.Hex), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

        }

        private void cencelButton_Click(object sender, RoutedEventArgs e)
        {
            MainLayout ds = new MainLayout(this.walletName);
            ds.Show();
        }

       
    }
}
