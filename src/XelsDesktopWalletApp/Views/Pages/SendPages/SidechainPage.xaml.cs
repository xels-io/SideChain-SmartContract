using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages.SendPages
{
    /// <summary>
    /// Interaction logic for SidechainPage.xaml
    /// </summary>
    public partial class SidechainPage : Page
    {
        private readonly string baseURL = URLConfiguration.BaseURL; // "http://localhost:37221/api";

        private readonly WalletInfo WalletInfo = new WalletInfo();
        private TransactionSending TransactionSending = new TransactionSending();
        private TransactionBuildingSidechain TransactionBuilding = new TransactionBuildingSidechain();

        private BuildTransaction BuildTransaction = new BuildTransaction();
        private WalletBalance WalletBalance = new WalletBalance();

        private double totalBalance;
        private string cointype;
        private double availableBalance;

        private double estimatedSidechainFee = 0;
        private bool isSending = false;
        private double opReturnAmount = 1;

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

        public SidechainPage()
        {
            InitializeComponent();
        }

        public SidechainPage(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.WalletInfo.WalletName = this.walletName;
            LoadCreateAsync();
        }

        public bool isAddrAmtValid()
        {
            //if (this.MainchainFederationAddressText.Text == string.Empty)
            //{
            //    MessageBox.Show("An address is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            //    this.MainchainFederationAddressText.Focus();
            //    return false;
            //}

            //if (this.MainchainFederationAddressText.Text.Length < 26)
            //{
            //    MessageBox.Show("An address is at least 26 characters long.");
            //    this.MainchainFederationAddressText.Focus();
            //    return false;
            //}

            if (this.SidechainDestinationAddressText.Text == string.Empty)
            {
                MessageBox.Show("An address is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.SidechainDestinationAddressText.Focus();
                return false;
            }

            if (this.SidechainDestinationAddressText.Text.Length < 26)
            {
                MessageBox.Show("An address is at least 26 characters long.");
                this.SidechainDestinationAddressText.Focus();
                return false;
            }

            return true;
        }

        public bool isValid()
        {
            if (isAddrAmtValid())
            {

                if (this.TransactionFeeText.Text == "")
                {
                    MessageBox.Show("Transaction Fee is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.TransactionFeeText.Focus();
                    return false;
                }

                if (this.password.Password == "")
                {
                    MessageBox.Show("Your password is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.password.Focus();
                    return false;
                }
            }

            return true;
        }

        public async void LoadCreateAsync()
        {
            await GetWalletBalanceAsync(this.baseURL);

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard ds = new Dashboard(this.walletName);
            ds.Show();
            // this.Close();
        }


        //private void TxtAmount_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (this.SidechainDestinationAddressText.Text != "" && this.SendAmountText.Text != "")
        //    {
        //        EstimateFeeSideChainAsync();
        //        this.SendAmountText.Focus();
        //    }
        //}

        //private void TxtAddress_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (this.SidechainDestinationAddressText.Text != "" && this.SendAmountText.Text != "")
        //    {
        //        EstimateFeeSideChainAsync();
        //        this.SidechainDestinationAddressText.Focus();
        //    }
        //}

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            this.isSending = true;
            BuildTransactionSideChainAsync();
        }

        private async Task GetWalletBalanceAsync(string path)
        {
            string getUrl = path + $"/wallet/balance?WalletName={this.WalletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var balances = JsonConvert.DeserializeObject<WalletBalanceArray>(content);

                foreach (var balance in balances.Balances)
                {
                    this.WalletBalance = balance;
                    this.AvailableBalanceText.Content = (balance.AmountConfirmed / 100000000).ToString();
                    this.CoinTypeText.Content = balance.CoinType.ToString();
                }

            }
            else
            {
                var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                foreach (var error in errors.Errors)
                {
                    MessageBox.Show(error.Message);
                }

            }
        }

        private async Task GetMaxBalanceAsync()
        {

            string postUrl = this.baseURL + $"/wallet/send-transaction";
            var content = "";

            MaximumBalance maximumBalance = new MaximumBalance();
            maximumBalance.WalletName = this.WalletInfo.WalletName;
            maximumBalance.AccountName = "account 0";
            maximumBalance.FeeType = "medium";
            maximumBalance.AllowUnconfirmed = true;

            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(maximumBalance), Encoding.UTF8, "application/json"));
            content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                this.estimatedSidechainFee = Convert.ToDouble(content) / 100000000;
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

        }

        private List<RecipientSidechain> GetRecipient()
        {

            List<RecipientSidechain> list = new List<RecipientSidechain>() {

               new RecipientSidechain{ DestinationAddress = this.SidechainDestinationAddressText.Text.Trim(),
               Amount = this.SendAmountText.Text}
            };
            return list;
        }


        private async void EstimateFeeSideChainAsync()
        {
            if (isAddrAmtValid())
            {
                this.TransactionFeeTypeLabel.Content = "medium";


                string postUrl = this.baseURL + $"/wallet/estimate-txfee";
                var content = "";

                FeeEstimationSideChain feeEstimation = new FeeEstimationSideChain();
                feeEstimation.WalletName = this.WalletInfo.WalletName;
                feeEstimation.AccountName = "account 0";
                feeEstimation.Recipients = GetRecipient();
                //feeEstimation.OpReturnData = this.SidechainDestinationAddressText.Text.Trim();
                feeEstimation.FeeType = this.TransactionFeeTypeLabel.Content.ToString();
                feeEstimation.AllowUnconfirmed = true;

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(feeEstimation), Encoding.UTF8, "application/json"));

                content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                   
                    this.estimatedSidechainFee = Convert.ToDouble(content) / 100000000;
                    this.TransactionFeeText.Text = this.estimatedSidechainFee.ToString();
                }
                else
                {
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach (var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
            }
        }

        private async void BuildTransactionSideChainAsync()
        {

            double t = this.opReturnAmount / 100000000;
            string postUrl = this.baseURL + $"/wallet/build-transaction";
            var content = "";

            this.TransactionBuilding.WalletName = this.WalletInfo.WalletName;
            this.TransactionBuilding.AccountName = "account 0";
            this.TransactionBuilding.Password = this.password.Password;
            this.TransactionBuilding.Recipients = GetRecipient();
            this.TransactionBuilding.FeeAmount = this.estimatedSidechainFee;
            this.TransactionBuilding.AllowUnconfirmed = true;
            this.TransactionBuilding.ShuffleOutputs = false;
            this.TransactionBuilding.OpReturnData = this.MainchainFederationAddressText.Text.Trim();
            this.TransactionBuilding.OpReturnAmount = (t).ToString("0." + new string('#', 10));

            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this.TransactionBuilding), Encoding.UTF8, "application/json"));

            content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                
                this.BuildTransaction = JsonConvert.DeserializeObject<BuildTransaction>(content);

                this.estimatedSidechainFee = this.BuildTransaction.Fee;
                this.TransactionSending.Hex = this.BuildTransaction.Hex;

                if (this.isSending)
                {
                    _ = SendTransactionAsync(this.TransactionSending);
                }
            }
            else
            {
                this.isSending = false; 
                var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                foreach (var error in errors.Errors)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        private async Task SendTransactionAsync(TransactionSending tranSending)
        {
            if (isValid())
            {
                string postUrl = this.baseURL + $"/wallet/send-transaction";
                
                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(tranSending), Encoding.UTF8, "application/json"));

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    SendConfirmationSC sendConfirmationSc = new SendConfirmationSC();
                    sendConfirmationSc.Transaction = this.TransactionBuilding;
                    sendConfirmationSc.TransactionFee = this.estimatedSidechainFee;
                    sendConfirmationSc.OpReturnAmount = this.opReturnAmount;
                    sendConfirmationSc.cointype = this.cointype;

                    SendConfirmationSideChain sendConf = new SendConfirmationSideChain(sendConfirmationSc, this.walletName);
                    sendConf.Show();
                    // this.Close();
                }
                else
                {
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach (var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
            }
        }

        private void CheckSendAmount_OnChange(object sender, RoutedEventArgs e)
        {
            double sendingAmount = Convert.ToDouble(this.SendAmountText.Text);

            if (this.SendAmountText.Text == string.Empty)
            {
                MessageBox.Show("An amount is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            if (sendingAmount < 0.00001)
            {
                MessageBox.Show("The amount has to be more or equal to 1.");

            }

            if (sendingAmount > ((this.WalletBalance.AmountConfirmed - this.estimatedSidechainFee) / 100000000))
            {
                MessageBox.Show("The total transaction amount exceeds your spendable balance.");

            }

            if (!Regex.IsMatch(this.SendAmountText.Text, @"^([0-9]+)?(\.[0-9]{0,8})?$"))
            {
                MessageBox.Show("Enter a valid transaction amount. Only positive numbers and no more than 8 doubles are allowed.");

            }

            this.SendAmountText.Focus();
        }


        private void CalculateTransactionFee_OnChange(object sender, RoutedEventArgs e)
        {
            if (this.SidechainDestinationAddressText.Text != "" && this.SendAmountText.Text != "")
            {
                EstimateFeeSideChainAsync();
                this.SidechainDestinationAddressText.Focus();
            }
        }



    }
}
