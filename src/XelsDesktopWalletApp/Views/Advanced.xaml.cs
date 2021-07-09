using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Advanced.xaml
    /// </summary>
    public partial class Advanced : Window
    {
        #region Base
        private string baseURL = URLConfiguration.BaseURLMain;// "http://localhost:37221/api";
        #endregion
        #region Wallet Info
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
        #endregion

        #region Landing
        public Advanced()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public Advanced(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            InitialViewLoad();
            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;

            // Extended public key
            GetExtPublicKey();

        }
        #endregion

        #region About 

        #endregion

        #region Ext Public Key


        public async void GetExtPublicKey()
        {
            await GetExtPublicAsync(this.baseURL);
        }

        private async Task GetExtPublicAsync(string path)
        {
            try
            {
                string getKey = path + $"/wallet/extpubkey?walletName={this.walletInfo.WalletName}&accountName=account 0";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getKey);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.ExtPubKeyTxt.Text = JsonConvert.DeserializeObject<string>(content);
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception r)
            {
                throw;
            }
        }


        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.ExtPubKeyTxt.Text);
        }

        #endregion

        #region Generate Addresses
        private void Generate_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Rescan
        private void Rescan_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Inner Navigation
        private void InitialViewLoad()
        {
            this.AboutSP.Visibility = Visibility.Visible;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Visible;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }
        private void ExtendedPublicKey_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Hidden;
            this.ExtendedPublicKeySP.Visibility = Visibility.Visible;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }
        private void GenerateAddresses_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Hidden;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Visible;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }
        private void Resync_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Hidden;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Visible;
        }
        #endregion

        #region Main Navigation
        private void Hyperlink_NavigateDashboard(object sender, RequestNavigateEventArgs e)
        {
            Dashboard ds = new Dashboard(this.walletName);
            ds.Show();
            this.Close();
        }
        private void Hyperlink_NavigateHistory(object sender, RequestNavigateEventArgs e)
        {
            History hs = new History(this.walletName);
            hs.Show();
            this.Close();
        }
        private void Hyperlink_NavigateExchange(object sender, RequestNavigateEventArgs e)
        {
            Exchange ex = new Exchange(this.walletName);
            ex.Show();
            this.Close();
        }
        private void Hyperlink_NavigateSmartContract(object sender, RequestNavigateEventArgs e)
        {
        }
        private void Hyperlink_NavigateLogout(object sender, RequestNavigateEventArgs e)
        {
            LogoutConfirm lc = new LogoutConfirm(this.walletName);
            lc.Show();
            this.Close();
        }
        private void Hyperlink_NavigateAddressBook(object sender, RequestNavigateEventArgs e)
        {
            AddressBook ex = new AddressBook(this.walletName);
            ex.Show();
            this.Close();
        }
        private void Hyperlink_NavigateAdvanced(object sender, RequestNavigateEventArgs e)
        {
            Advanced adv = new Advanced(this.walletName);
            adv.Show();
            this.Close();
        }
        #endregion

    }
}
