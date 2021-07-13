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
    /// Interaction logic for MainchainPage.xaml
    /// </summary>
    public partial class MainchainPage : Page
    {
        private readonly string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";
        private readonly WalletInfo walletInfo = new WalletInfo();


        private TransactionSending transactionSending = new TransactionSending();
        private TransactionBuilding transactionBuilding = new TransactionBuilding();

        private WalletBalanceArray balances = new WalletBalanceArray();
        private BuildTransaction buildTransaction = new BuildTransaction();

        private decimal totalBalance;
        private string cointype;
        private decimal spendableBalance;

        private decimal estimatedFee = 0;
        private bool isSending = false;

        private decimal opReturnAmount = 1;

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
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            LoadCreateAsync();
        }



        private void TxtAmount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.textSidechainDestinationAddress.Text != "" && this.SendAmountText.Text != "")
            {
                EstimateFeeAsync();
                this.SendAmountText.Focus();
            }
        }
        private void TxtAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.textSidechainDestinationAddress.Text != "" && this.SendAmountText.Text != "")
            {
                EstimateFeeAsync();
                this.textSidechainDestinationAddress.Focus();
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            this.isSending = true;
            _ = BuildTransactionAsync();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard ds = new Dashboard(this.walletName);
            ds.Show();
            //this.Close();
        }

        public bool IsAddressAndAmountValid()
        {
            if (this.textSidechainDestinationAddress.Text == string.Empty)
            {
                MessageBox.Show("An address is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.textSidechainDestinationAddress.Focus();
                return false;
            }

            if (this.textSidechainDestinationAddress.Text.Length < 26)
            {
                MessageBox.Show("An address is at least 26 characters long.");
                this.textSidechainDestinationAddress.Focus();
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

            if (this.SendAmountText.Text.Length > ((this.spendableBalance - this.estimatedFee) / 100000000))
            {
                MessageBox.Show("The total transaction amount exceeds your spendable balance.");
                this.SendAmountText.Focus();
                return false;
            }

            if (!Regex.IsMatch(this.SendAmountText.Text, @"^([0-9]+)?(\.[0-9]{0,8})?$"))
            {
                MessageBox.Show("Enter a valid transaction amount. Only positive numbers and no more than 8 decimals are allowed.");
                this.SendAmountText.Focus();
                return false;
            }

            return true;
        }

        public bool isValid()
        {
            if (IsAddressAndAmountValid())
            {

                if (this.textTransactionFee.Text == "")
                {
                    MessageBox.Show("Transaction Fee is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.textTransactionFee.Focus();
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

        private async Task GetWalletBalanceAsync(string path)
        {
            string getUrl = path + $"/wallet/balance?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);


            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();

                this.balances = JsonConvert.DeserializeObject<WalletBalanceArray>(content);

                this.totalBalance = this.balances.Balances[0].AmountConfirmed + this.balances.Balances[0].AmountUnconfirmed;
                this.cointype = this.balances.Balances[0].CoinType;
                this.spendableBalance = this.balances.Balances[0].SpendableAmount;

                this.textAvailableCoin.Content = this.totalBalance.ToString();
                // this.textCoinType.Content = this.cointype.ToString();

            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }

        private async Task GetMaxBalanceAsync()
        {

            string postUrl = this.baseURL + $"/wallet/send-transaction";
            var content = "";

            MaximumBalance maximumBalance = new MaximumBalance();
            maximumBalance.WalletName = this.walletInfo.WalletName;
            maximumBalance.AccountName = "account 0";
            maximumBalance.FeeType = "medium";
            maximumBalance.AllowUnconfirmed = true;

            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(maximumBalance), Encoding.UTF8, "application/json"));


            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
                //this.estimatedFee = Money.Parse(content);
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

        }

        private Recipient[] GetRecipient()
        {

            Recipient[] recipients = new Recipient[1];

            recipients[0].DestinationAddress = this.textSidechainDestinationAddress.Text.Trim();
            recipients[0].Amount = this.SendAmountText.Text;

            return recipients;
        }

        private async void EstimateFeeAsync()
        {
            if (IsAddressAndAmountValid())
            {
                this.textTransactionFee.Text = "medium";
                Recipient[] recipients = GetRecipient();

                string postUrl = this.baseURL + $"/wallet/estimate-txfee";
                var content = "";

                FeeEstimation feeEstimation = new FeeEstimation();
                feeEstimation.walletName = this.walletInfo.WalletName;
                feeEstimation.accountName = "account 0";
                feeEstimation.recipients = recipients;
                feeEstimation.feeType = this.textTransactionFee.Text;
                feeEstimation.allowUnconfirmed = true;

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(feeEstimation), Encoding.UTF8, "application/json"));


                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    //this.estimatedFee = Money.Parse(content);
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }

        }

        private async Task BuildTransactionAsync()
        {
            Recipient[] recipients = GetRecipient();


            string postUrl = this.baseURL + $"/wallet/build-transaction";
            var content = "";

            this.transactionBuilding.WalletName = this.walletInfo.WalletName;
            this.transactionBuilding.AccountName = "account 0";
            this.transactionBuilding.Password = this.password.Password;
            this.transactionBuilding.Recipients = recipients;
            this.transactionBuilding.FeeAmount = this.estimatedFee / 100000000;
            this.transactionBuilding.AllowUnconfirmed = true;
            this.transactionBuilding.ShuffleOutputs = false;


            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this.transactionBuilding), Encoding.UTF8, "application/json"));


            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
                this.buildTransaction = JsonConvert.DeserializeObject<BuildTransaction>(content);

                this.estimatedFee = this.buildTransaction.Fee;
                this.transactionSending.Hex = this.buildTransaction.Hex;

                if (this.isSending)
                {
                    _ = SendTransactionAsync(this.transactionSending);
                }

            }
            else
            {
                this.isSending = false;
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

        }

        private async Task SendTransactionAsync(TransactionSending tranSending)
        {
            if (isValid())
            {
                string postUrl = this.baseURL + $"/wallet/send-transaction";
                // var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(tranSending), Encoding.UTF8, "application/json"));


                if (response.IsSuccessStatusCode)
                {
                    // content = await response.Content.ReadAsStringAsync();

                    SendConfirmation sendConfirmation = new SendConfirmation();
                    sendConfirmation.Transaction = this.transactionBuilding;
                    sendConfirmation.TransactionFee = this.estimatedFee;
                    sendConfirmation.Cointype = this.cointype;

                    SendConfirmationMainChain sendConf = new SendConfirmationMainChain(sendConfirmation, this.walletName);
                    sendConf.Show();
                    // this.Close();

                }
                else
                {
                    this.isSending = false;
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }

        }

        private void CalculateTransactionFee_OnChange(object sender, RoutedEventArgs e)
        {
            var amount = this.SendAmountText.Text;
        }


    }
}
