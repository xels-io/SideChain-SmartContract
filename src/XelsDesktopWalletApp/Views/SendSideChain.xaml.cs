using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using NBitcoin;

using Newtonsoft.Json;

using Xels.Bitcoin.Features.Wallet;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for SendSideChain.xaml
    /// </summary>
    public partial class SendSideChain : Window
    {

        private readonly string baseURL = URLConfiguration.BaseURL; // "http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();
        private TransactionSending transactionSending = new TransactionSending();
        private TransactionBuildingSidechain transactionBuilding = new TransactionBuildingSidechain();

        private WalletBalanceArray balances = new WalletBalanceArray();
        private BuildTransaction buildTransaction = new BuildTransaction();

        private double totalBalance;
        private string cointype;
        private double spendableBalance;

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

        public SendSideChain()
        {
            InitializeComponent();
        }

        public SendSideChain(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            LoadCreateAsync();
        }

        public bool isAddrAmtValid()
        {
            if (this.textMainchainFederationAddress.Text == string.Empty)
            {
                MessageBox.Show("An address is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.textMainchainFederationAddress.Focus();
                return false;
            }

            if (this.textMainchainFederationAddress.Text.Length < 26)
            {
                MessageBox.Show("An address is at least 26 characters long.");
                this.textMainchainFederationAddress.Focus();
                return false;
            }

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

            if (this.textAmount.Text == string.Empty)
            {
                MessageBox.Show("An amount is required.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.textAmount.Focus();
                return false;
            }

            if (this.textAmount.Text.Length < 0.00001)
            {
                MessageBox.Show("The amount has to be more or equal to 1.");
                this.textAmount.Focus();
                return false;
            }

            if (this.textAmount.Text.Length > ((this.spendableBalance - this.estimatedSidechainFee) / 100000000))
            {
                MessageBox.Show("The total transaction amount exceeds your spendable balance.");
                this.textAmount.Focus();
                return false;
            }

            if (!Regex.IsMatch(this.textAmount.Text, @"^([0-9]+)?(\.[0-9]{0,8})?$"))
            {
                MessageBox.Show("Enter a valid transaction amount. Only positive numbers and no more than 8 doubles are allowed.");
                this.textAmount.Focus();
                return false;
            }

            return true;
        }

        public bool isValid()
        {
            if (isAddrAmtValid())
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
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard ds = new Dashboard(this.walletName);
            ds.Show();
            this.Close();
        }

        private void XELS_Button_Click(object sender, RoutedEventArgs e)
        {
            Send send = new Send(this.walletName);
            send.Show();
            this.Close();
        }

        private void SELS_Button_Click(object sender, RoutedEventArgs e)
        {
            SendSelsBels sendsb = new SendSelsBels(this.walletName);
            sendsb.Show();
            this.Close();
        }

        private void BELS_Button_Click(object sender, RoutedEventArgs e)
        {
            SendSelsBels sendsb = new SendSelsBels(this.walletName);
            sendsb.Show();
            this.Close();
        }

        private void Mainchain_Button_Click(object sender, RoutedEventArgs e)
        {

            Send send = new Send(this.walletName);
            send.Show();
            this.Close();
        }

        private void Sidechain_Button_Click(object sender, RoutedEventArgs e)
        {
            SendSideChain sendSC = new SendSideChain(this.walletName);
            sendSC.Show();
            this.Close();

        }

        private void TxtAmount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.textSidechainDestinationAddress.Text != "" && this.textAmount.Text != "")
            {
                EstimateFeeSideChainAsync();
                this.textAmount.Focus();
            }
        }

        private void TxtAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.textSidechainDestinationAddress.Text != "" && this.textAmount.Text != "")
            {
                EstimateFeeSideChainAsync();
                this.textSidechainDestinationAddress.Focus();
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            this.isSending = true;
            BuildTransactionSideChainAsync();
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
                this.textCoinType.Content = this.cointype.ToString();

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
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
            //this.estimatedSidechainFee = Money.Parse(content);

        }

        private RecipientSidechain[] GetRecipient()
        {

            RecipientSidechain[] recipients = new RecipientSidechain[1];

            recipients[0].FederationAddress = this.textMainchainFederationAddress.Text.Trim();
            recipients[0].Amount = this.textAmount.Text;

            return recipients;
        }


        private async void EstimateFeeSideChainAsync()
        {
            if (isAddrAmtValid())
            {
                this.textTransactionFee.Text = "medium";
                RecipientSidechain[] recipients = GetRecipient();

                string postUrl = this.baseURL + $"/wallet/estimate-txfee";
                var content = "";

                FeeEstimationSideChain feeEstimation = new FeeEstimationSideChain();
                feeEstimation.WalletName = this.walletInfo.WalletName;
                feeEstimation.AccountName = "account 0";
                //feeEstimation.Recipients = GetRecipient();
                feeEstimation.OpReturnData = this.textSidechainDestinationAddress.Text.Trim();
                feeEstimation.FeeType = this.textTransactionFee.Text;
                feeEstimation.AllowUnconfirmed = true;

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(feeEstimation), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                   // this.estimatedSidechainFee = Money.Parse(content);
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
        }

        private async void BuildTransactionSideChainAsync()
        {             
            RecipientSidechain[] recipients = GetRecipient();

            string postUrl = this.baseURL + $"/wallet/build-transaction";
            var content = "";

            this.transactionBuilding.WalletName = this.walletInfo.WalletName;
            this.transactionBuilding.AccountName = "account 0";
            this.transactionBuilding.Password = this.password.Password;
            //this.transactionBuilding.Recipients = recipients;
            //this.transactionBuilding.FeeAmount = this.estimatedSidechainFee / 100000000;
            this.transactionBuilding.AllowUnconfirmed = true;
            this.transactionBuilding.ShuffleOutputs = false;
            this.transactionBuilding.OpReturnData = this.textSidechainDestinationAddress.Text.Trim();
            //this.transactionBuilding.OpReturnAmount = this.opReturnAmount / 100000000;

            HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this.transactionBuilding), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
                this.buildTransaction = JsonConvert.DeserializeObject<BuildTransaction>(content);

                this.estimatedSidechainFee = this.buildTransaction.Fee;
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

                    SendConfirmationSC sendConfirmationSc = new SendConfirmationSC();
                    sendConfirmationSc.Transaction = this.transactionBuilding;
                    sendConfirmationSc.TransactionFee = this.estimatedSidechainFee;
                    sendConfirmationSc.OpReturnAmount = this.opReturnAmount;
                    sendConfirmationSc.cointype = this.cointype;

                    this.Content = new SendConfirmationSideChain(sendConfirmationSc, this.walletName);
                    //sendConf.Show();
                    //this.Close();
                }
                else
                {
                    MessageBox.Show($"Error Code{ response.StatusCode } : Message - { response.ReasonPhrase}");
                }
            }
        }

    }
}
