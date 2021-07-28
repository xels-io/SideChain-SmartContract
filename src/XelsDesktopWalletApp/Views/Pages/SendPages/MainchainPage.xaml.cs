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
using XelsDesktopWalletApp.Models.SmartContractModels;
using XelsDesktopWalletApp.Views.layout;
using XelsDesktopWalletApp.Views.Pages.Modals;

namespace XelsDesktopWalletApp.Views.Pages.SendPages
{

    public partial class MainchainPage : Page
    {
        private readonly string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";
        private readonly WalletInfo walletInfo = new WalletInfo();


        private TransactionSending TransactionSending = new TransactionSending();
        private TransactionBuilding TransactionBuilding = new TransactionBuilding();
        private WalletBalance WalletBalance = new WalletBalance();

        private BuildTransaction BuildTransaction = new BuildTransaction();
        private string cointype; 

        private double estimatedFee = 0;
        private bool isSending = false;
         

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


        public MainchainPage()
        {
            InitializeComponent();
        }

        public MainchainPage(string walletname)
        {

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            InitializeComponent();
            this.DataContext = this;
            this.DestinationAddressText.Text = GlobalPropertyModel.selectAddressFromAddressBook;
          
            _= GetMaxBalanceAsync();
        }

      
         
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            this.isSending = true;
            _ = BuildTransactionAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        { 
            this.Visibility = Visibility.Collapsed;
        }

        public bool IsAddressAndAmountValid()
        {
            if (this.DestinationAddressText.Text == string.Empty)
            {
                MessageBox.Show("An address is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DestinationAddressText.Focus();
                return false;
            }

            if (this.DestinationAddressText.Text.Length < 26)
            {
                MessageBox.Show("An address is at least 26 characters long.");
                this.DestinationAddressText.Focus();
                return false;
            }

            if (this.SendAmountText.Text == string.Empty)
            {
                MessageBox.Show("An amount is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.SendAmountText.Focus();
                return false;
            }

            if (this.SendAmountText.Text.Length < 0.00001)
            {
                MessageBox.Show("The amount has to be more or equal to 1.");
                this.SendAmountText.Focus();
                return false;
            }           

            return true;
        }

        public bool isValid()
        {
            if (IsAddressAndAmountValid())
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

        
        //private async Task GetWalletBalanceAsync()
        //{
        //    string getUrl = this.baseURL + $"/wallet/balance?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
        //    var content = "";

        //    HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

        //    content = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //    {        
        //        var balances = JsonConvert.DeserializeObject<WalletBalanceArray>(content);

        //        foreach(var balance in balances.Balances)
        //        {
        //            this.WalletBalance = balance;
        //            this.textAvailableCoin.Content = $"{(balance.AmountConfirmed / 100000000).ToString()} {GlobalPropertyModel.CoinUnit}"; 
        //        } 
        //    }
        //    else
        //    {
        //        MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
        //    }
        //}

        private async Task GetMaxBalanceAsync()
        {            
            var content = "";

            MaximumBalance maximumBalance = new MaximumBalance();
            maximumBalance.WalletName = this.walletInfo.WalletName;
           // maximumBalance.AccountName = "account 0";
            maximumBalance.FeeType = "medium";
            maximumBalance.AllowUnconfirmed = true;

            string postUrl = this.baseURL + 
                $"/wallet/maximumbalancewpf?WalletName={maximumBalance.WalletName}&FeeType={maximumBalance.FeeType}&AllowUnconfirmed={maximumBalance.AllowUnconfirmed}" ;
            //&accountName={maximumBalance.AccountName}
            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(postUrl);

            content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var balance = JsonConvert.DeserializeObject<WalletBalance>(content);

                //foreach (var balance in balances.Balances)
                {
                    this.WalletBalance = balance;
                    
                    this.textAvailableCoin.Content = $"{(balance.MaxSpendableAmount / 100000000).ToString()} {GlobalPropertyModel.CoinUnit}";

                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

        }

        private List<Recipient> GetRecipient()
        {

            List<Recipient> list = new List<Recipient>() {

               new Recipient{ DestinationAddress = this.DestinationAddressText.Text.Trim(),
               Amount = this.SendAmountText.Text}
            }; 

            return list;
        }

        private async void EstimateFeeAsync()
        {
            if (IsAddressAndAmountValid())
            {
                this.TransactionFeeTypeLabel.Content = "medium";               
              
                string postUrl = this.baseURL + $"/wallet/estimate-txfee";
                var content = "";

                FeeEstimation feeEstimation = new FeeEstimation();
                feeEstimation.WalletName = this.walletInfo.WalletName;
                feeEstimation.AccountName = "account 0";
                feeEstimation.Recipients = GetRecipient();
                feeEstimation.FeeType = this.TransactionFeeTypeLabel.Content.ToString();
                feeEstimation.AllowUnconfirmed = true;

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(feeEstimation), Encoding.UTF8, "application/json"));

                content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    this.estimatedFee = (Convert.ToDouble(content) / 100000000);
                    this.TransactionFeeText.Text = (Convert.ToDouble(content) /100000000).ToString();
                    this.TransactionWarningLabel.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }

        }

        private async Task BuildTransactionAsync()
        {
            //Recipient[] recipients = GetRecipient();


            string postUrl = this.baseURL + $"/wallet/build-transaction";
            var content = "";

            this.TransactionBuilding.WalletName = this.walletInfo.WalletName;
            this.TransactionBuilding.AccountName = "account 0";
            this.TransactionBuilding.Password = this.password.Password;
            this.TransactionBuilding.Recipients = GetRecipient();
            this.TransactionBuilding.FeeAmount = this.estimatedFee;
            this.TransactionBuilding.AllowUnconfirmed = true;
            this.TransactionBuilding.ShuffleOutputs = false;


            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this.TransactionBuilding), Encoding.UTF8, "application/json"));

            content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            { 
                    this.BuildTransaction = JsonConvert.DeserializeObject<BuildTransaction>(content);

                this.estimatedFee = this.BuildTransaction.Fee;
                this.TransactionSending.Hex = this.BuildTransaction.Hex;

                if (this.isSending)
                {
                    _ = SendTransactionAsync(this.TransactionSending);
                }

            }
            else
            {
                this.isSending = false;

                try
                {
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach (var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);

                        //MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                    }
                }
                catch (Exception e)
                {

                    throw;
                }
                
                
            }

        }

        private async Task SendTransactionAsync(TransactionSending tranSending)
        {
            if (isValid())
            {
                string postUrl = this.baseURL + $"/wallet/send-transaction";
                 var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(tranSending), Encoding.UTF8, "application/json"));

                 content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {        
                    SendConfirmation sendConfirmation = new SendConfirmation();

                    sendConfirmation.Transaction = this.TransactionBuilding;
                    sendConfirmation.TransactionFee = this.estimatedFee;
                    sendConfirmation.Cointype = this.cointype;


                    this.NavigationService.Navigate(new SendConfirmationMainChain(sendConfirmation, this.walletName));
                    //SendConfirmationMainChain sendConf = new SendConfirmationMainChain(sendConfirmation, this.walletName);
                    //sendConf.Show();
                    // this.Close();

                }
                else
                {
                    this.isSending = false;
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach (var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);

                        //MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                    }
                }
            }

        }


        private void CheckSendAmount_OnChange(object sender, RoutedEventArgs e)
        {
            string sendingAmount = this.SendAmountText.Text ;

            if (!string.IsNullOrWhiteSpace(sendingAmount))
            {
                if (Convert.ToDouble(sendingAmount) > ((this.WalletBalance.MaxSpendableAmount - this.WalletBalance.Fee) / 100000000))
                {
                    MessageBox.Show("The total transaction amount exceeds your spendable balance.");
                }

                if (!Regex.IsMatch(this.SendAmountText.Text, @"^([0-9]+)?(\.[0-9]{0,8})?$"))
                {
                    MessageBox.Show("Enter a valid transaction amount. Only positive numbers and no more than 8 decimals are allowed.");
                }

                if (this.DestinationAddressText.Text != "" && this.SendAmountText.Text != "")
                {
                    EstimateFeeAsync();
                    this.DestinationAddressText.Focus();
                }
            }

            this.SendAmountText.Focus();
        }


        private void CalculateTransactionFee_OnChange(object sender, RoutedEventArgs e)
        {
            if (this.DestinationAddressText.Text != "" && this.SendAmountText.Text != "")
            {
                EstimateFeeAsync();
                this.DestinationAddressText.Focus();
            }
        }



    }
}
