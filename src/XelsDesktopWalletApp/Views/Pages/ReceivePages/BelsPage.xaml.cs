using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Models.SmartContractModels;

namespace XelsDesktopWalletApp.Views.Pages.ReceivePages
{
    /// <summary>
    /// Interaction logic for BelsPage.xaml
    /// </summary>
    public partial class BelsPage : Page
    {
        private CreateWallet createWallet = new CreateWallet();
        string baseURL = URLConfiguration.BaseURL;  //"http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();
        QRCodeConverter QRCode = new QRCodeConverter();

        private string walletName = GlobalPropertyModel.WalletName;

        public BelsPage()
        {
            InitializeComponent();
            this.DataContext = this;
          
            //LoadCreate();
            GenerateQRCode();
            this.BelsAddress();
        }

        public async void LoadCreate()
        {
            string addr = await GetAPIAsync(this.baseURL);
            addr = FreshAddress(addr);

            this.textBoxTextToQr.Text = addr;
        }

        private string FreshAddress(string adr)
        {
            adr = adr.Trim(new Char[] { '"' });
            return adr;
        }

        private void GenerateQRCode()
        {
            this.image.Source = this.QRCode.GenerateQRCode(this.textBoxTextToQr.Text);
        }

        private void CopyAddressButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.textBoxTextToQr.Text);
            MessageBox.Show("Address Copied Successfully :- " + this.textBoxTextToQr.Text.ToString());
        }
         
        private async Task<string> GetAPIAsync(string path)
        {
            string getUrl = path + $"/wallet/unusedaddress?WalletName={this.walletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);


            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            return content;

        }

        private void BelsAddress()
        {
            try
            {
                string walletCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string path = walletCurrentDirectory + @"\File\Wallets.json";

                if (File.Exists(path))
                { 
                    StoredWallet belswallet = this.createWallet.GetLocalWallet(this.walletName, "BELS");

                    this.textBoxTextToQr.Text = belswallet.Address;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("");
            }
        }

    }
}
