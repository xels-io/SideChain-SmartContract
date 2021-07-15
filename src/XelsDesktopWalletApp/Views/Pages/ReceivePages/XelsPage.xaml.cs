using System;
using System.Collections.Generic;
using System.Drawing;
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

using Newtonsoft.Json;

using QRCoder;

using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages.ReceivePages
{
    /// <summary>
    /// Interaction logic for XelsPage.xaml
    /// </summary>
    public partial class XelsPage : Page
    {

        string baseURL = URLConfiguration.BaseURL; //  "http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();

        private ReceiveWalletStatus ReceiveWalletStatus;

        QRCodeConverter QRCode = new QRCodeConverter();

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

        public XelsPage()
        {
            InitializeComponent();
            DataContext = this;

        }

        public XelsPage(string walletname)
        {
            InitializeComponent();
            this.ReceiveWalletStatus = new ReceiveWalletStatus();
            this.ReceiveWalletStatus.UsedAddresses = new List<ReceiveWalletStatus>();
            this.ReceiveWalletStatus.UnusedAddresses = new List<ReceiveWalletStatus>();
            this.ReceiveWalletStatus.ChangedAddresses = new List<ReceiveWalletStatus>();
            this.walletName = walletname;

            DataContext = this;

            this.walletInfo.WalletName = this.walletName;
            LoadCreate();
            GenerateQRCode();
        }

        public async Task LoadCreate()
        {
            string addr = await GetUnusedReceivedAddrAPIAsync();
            addr = FreshAddress(addr);

            this.textBoxTextToQr.Text = addr;
        }
        private string FreshAddress(string adr)
        {
            adr = adr.Trim(new char[] { '"' });
            return adr;
        }

        private async Task<string> GetUnusedReceivedAddrAPIAsync()
        {
            string getUrl = this.baseURL + $"/wallet/unusedaddress?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
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

        private async Task GetAllAddresses()
        {

            string getUrl = this.baseURL + $"/wallet/addresses?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
            content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {

                try
                {
                    var addresses = JsonConvert.DeserializeObject<ReceiveWalletArray>(content);

                    foreach (var address in addresses.Addresses)
                    {
                        if (address.IsUsed)
                        {
                            this.ReceiveWalletStatus.UsedAddresses.Add(address);

                            this.UsedAddressList.ItemsSource = this.ReceiveWalletStatus.UsedAddresses;
                        }
                        else if (address.IsChange)
                        {
                            this.ReceiveWalletStatus.ChangedAddresses.Add(address);

                            this.ChangedAddressList.ItemsSource = this.ReceiveWalletStatus.ChangedAddresses;
                        }
                        else
                        {
                            this.ReceiveWalletStatus.UnusedAddresses.Add(address);
                            //this.UnusedAddressList.ItemsSource = this.ReceiveWalletStatus.UnusedAddresses;
                        }

                    }
                }
                catch (Exception e)
                {

                    throw;
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }

        private void ShowAllAddressButton_Click(object sender, RoutedEventArgs e)
        {
            GetAllAddresses();
            this.AllAddressList.Visibility = Visibility.Visible;
            this.BackSingleAddressButton.Visibility = Visibility.Visible;
            this.SingleAddress.Visibility = Visibility.Hidden;
        }


        private void GenerateQRCode()
        {
            this.image.Source = QRCode.GenerateQRCode(this.textBoxTextToQr.Text);
        }

        private void SingleAddress_Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.textBoxTextToQr.Text);
        }

        private void Address_Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            string item = ((sender as Button)?.Tag as ListViewItem)?.ToString();
            Clipboard.SetText(item);

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ReceiveShowall rsa = new ReceiveShowall(this.walletName);
            rsa.Show();

        }

        private void BackSingleAddressButton_Click(object sender, RoutedEventArgs e)
        {
            this.AllAddressList.Visibility = Visibility.Hidden;
            this.SingleAddress.Visibility = Visibility.Visible;
            this.BackSingleAddressButton.Visibility = Visibility.Hidden;
        }

    }
}
