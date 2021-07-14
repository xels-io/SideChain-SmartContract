using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Newtonsoft.Json;
using Xels.Bitcoin.Controllers.Models;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for AdvancedPage.xaml
    /// </summary>
    public partial class AdvancedPage : Page
    {
        #region Base
        private string baseURL = URLConfiguration.BaseURLMain;// "http://localhost:37221/api";
        private string version = Applicationversion.Version;
        #endregion
        #region Local Info

        private StatusModel statusModel = new StatusModel();
        private string amount;
        private string[] addresses;
        private WalletGeneralInfoModel walletGeneralInfoModel = new WalletGeneralInfoModel();
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
        public AdvancedPage()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public AdvancedPage(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            InitialViewLoad();
            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;

            // About 
            this.WalletversionTxt.Text = this.version;
            GetAbout();
            // Extended-public-key
            GetExtPublicKey();
            //Rescan 
            GetGeneralInfo();
        }
        #endregion

        #region Validations

        // Generate Addresses
        public bool isValidGA()
        {
            if (this.AmountofGenerateAddressesTxt.Text == string.Empty)
            {
                MessageBox.Show("Please enter an amount to generate.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.AmountofGenerateAddressesTxt.Focus();
                return false;
            }

            if (this.AmountofGenerateAddressesTxt.Text.Length < 1)
            {
                MessageBox.Show("Please generate at least one address.");
                this.AmountofGenerateAddressesTxt.Focus();
                return false;
            }

            if (this.AmountofGenerateAddressesTxt.Text.Length > 1000)
            {
                MessageBox.Show("You can only generate 1000 addresses at once.");
                this.AmountofGenerateAddressesTxt.Focus();
                return false;
            }

            if (!Regex.IsMatch(this.AmountofGenerateAddressesTxt.Text, @"^[0-9]*$"))
            {
                MessageBox.Show("Please enter a number between 1 and 10.");
                this.AmountofGenerateAddressesTxt.Focus();
                return false;
            }

            return true;
        }

        // Rescan Wallet
        public bool isValidRW()
        {
            if (this.RescanFromDate.SelectedDate == null)
            {
                MessageBox.Show("Please choose the date the wallet should sync from.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.RescanFromDate.Focus();
                return false;
            }

            //if (Synced.IsSynced == true)
            //{
            //    MessageBox.Show("Syncing..", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            //    this.RescanFromDate.Focus();
            //    return false;
            //}

            return true;
        }

        #endregion

        #region About 

        public async void GetAbout()
        {
            await GetNodeStatusIntervalAsync(this.baseURL);
        }

        private async Task GetNodeStatusIntervalAsync(string path)
        {
            try
            {
                string getNodeStatusInterval = path + $"/node/status";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getNodeStatusInterval);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.statusModel = JsonConvert.DeserializeObject<StatusModel>(content);

                    this.ClientnameTxt.Text = this.statusModel.Agent;
                    this.FullnodeversionTxt.Text = this.statusModel.Version;
                    this.CurrentnetworkTxt.Text = this.statusModel.Network;
                    this.ProtocolversionTxt.Text = this.statusModel.ProtocolVersion.ToString();
                    this.CurrentblockheightTxt.Text = this.statusModel.BlockStoreHeight.ToString();
                    this.WalletdatadirectoryTxt.Text = this.statusModel.DataDirectoryPath;
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
            this.amount = this.AmountofGenerateAddressesTxt.Text;

            if (isValidGA())
            {
                GetUnusedReceiveAddresses();
            }
        }

        public async void GetUnusedReceiveAddresses()
        {
            await GetUnusedReceiveAddressesAsync(this.baseURL);
        }

        private async Task GetUnusedReceiveAddressesAsync(string path)
        {
            try
            {
                string getUnusedReceiveAddresses = path + $"/wallet/unusedaddresses?walletName={this.walletInfo.WalletName}&accountName=account 0&count={this.amount}";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUnusedReceiveAddresses);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.addresses = JsonConvert.DeserializeObject<string[]>(content);
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
        #endregion

        #region Rescan

        public async void GetGeneralInfo()
        {
            await GetGeneralWalletInfoAsync(this.baseURL);
        }

        private async Task GetGeneralWalletInfoAsync(string path)
        {
            try
            {
                string getUrl = path + $"/wallet/general-info?Name={this.walletInfo.WalletName}";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.walletGeneralInfoModel = JsonConvert.DeserializeObject<WalletGeneralInfoModel>(content);

                    if (this.walletGeneralInfoModel.IsChainSynced
                        && this.walletGeneralInfoModel.LastBlockSyncedHeight == this.walletGeneralInfoModel.ChainTip)
                    {
                        Synced.IsSynced = false;
                    }
                    else
                    {
                        Synced.IsSynced = true;
                    }
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void Rescan_Click(object sender, RoutedEventArgs e)
        {
            string fromdate = this.RescanFromDate.SelectedDate.ToString();
            if (isValidRW())
            {
                RemoveTransactionsAsync(this.baseURL, fromdate);
            }
        }

        private async Task RemoveTransactionsAsync(string path, string date)
        {
            try
            {
                string removeUrl = path + $"/wallet/remove-transactions/?walletName={this.walletInfo.WalletName}&fromDate={date}&reSync=true";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.DeleteAsync(removeUrl);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();

                    MessageBox.Show("Your wallet is now resyncing. The time remaining depends on the size and creation time of your wallet. The wallet dashboard shows your progress.");
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception e)
            {
                throw;
            }
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
    }
}
