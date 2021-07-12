using System;
using System.Collections.Generic;
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
using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for ExchangePage.xaml
    /// </summary>
    public partial class ExchangePage : Page
    {


        #region Base
        private string baseURL = URLConfiguration.BaseURL;
        private string baseURLExchange = URLConfiguration.BaseURLExchange;
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

        string addr;
        bool updatesuccess = false;
        private readonly StoredWallet mywallet = new StoredWallet();
        private CreateWallet createWallet = new CreateWallet();
        public List<ExchangeResponse> exchangedatalist = new List<ExchangeResponse>();

        #region Exchanges
        public List<ExchangeData> exchangelist = new List<ExchangeData>();

        public List<ExchangeData> Exchangelist
        {
            get
            {
                return this.exchangelist;
            }
            set
            {
                this.exchangelist = value;
            }
        }
        #endregion
        #region Coin Select

        private ExchangeCoin selectedcoin = new ExchangeCoin();
        public ExchangeCoin SelectedCoin
        {
            get
            {
                return this.selectedcoin;
            }
            set
            {
                this.selectedcoin = value;
            }
        }

        private List<ExchangeCoin> coins = new List<ExchangeCoin>() {
                new ExchangeCoin(){ Name="SELS"},
                new ExchangeCoin(){ Name="BELS"}
            };

        public List<ExchangeCoin> Coins
        {
            get
            {
                return this.coins;
            }
            set
            {
                this.coins = value;
            }
        }

        #endregion
        public ExchangePage()
        {
            InitializeComponent();
        }
        public ExchangePage(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            this.mywallet = this.createWallet.GetLocalWalletDetails(this.walletInfo.WalletName);
            Task.Run(async () => await LoadCreateAsync());
            Task.Run(async () => await UpdateExchangeListAsync());
            this.updatesuccess = false;
        }


        public bool isValid()
        {
            if (this.Combobox.SelectedItem == null)
            {
                MessageBox.Show("Deposit From is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Combobox.Focus();
                return false;
            }

            if (this.AmountTxt.Text == string.Empty)
            {
                MessageBox.Show("Deposit Amount is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.AmountTxt.Focus();
                return false;
            }

            return true;
        }


        #region Api Requests
        public async Task<List<ExchangeResponse>> GetOrdersAsync(string hash)
        {
            try
            {
                string postUrl = this.baseURLExchange + "/api/getOrders";
                var content = "";
                HttpClient client = new HttpClient();

                List<ExchangeResponse> exchangedata = new List<ExchangeResponse>();
                PostHash code = new PostHash();
                code.user_code = hash;

                client.DefaultRequestHeaders.Add("Authorization", "1234567890");

                HttpResponseMessage response = await client.PostAsJsonAsync(postUrl, code).ConfigureAwait(false);
                content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    exchangedata = JsonConvert.DeserializeObject<List<ExchangeResponse>>(content);
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }

                return exchangedata;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task NewOrderAsync(ExchangeOrder order)
        {
            try
            {
                string postUrl = URLConfiguration.BaseURLExchange + "/api/new-order";

                var content = "";
                ExchangeResponse exchangedata = new ExchangeResponse();
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("Authorization", "1234567890");

                HttpResponseMessage response = await client.PostAsJsonAsync(postUrl, order);
                content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    exchangedata = JsonConvert.DeserializeObject<ExchangeResponse>(content);

                    await UpdateExchangeListAsync();

                    //if (this.updatesuccess == true) {
                    await DepositAsync(order.deposit_symbol, order.deposit_amount, exchangedata);
                    //}

                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }
        #endregion

        public async Task UpdateExchangeListAsync()
        {

                if (this.mywallet != null)
                {
                    if (this.mywallet.Wallethash != "")
                    {
                        try
                        {

                        this.exchangedatalist = await GetOrdersAsync(this.mywallet.Wallethash);
                        this.Dispatcher.Invoke(() =>
                        {

                            if (this.exchangedatalist != null && this.exchangedatalist.Count > 0)
                            {
                                this.ExchangesList.ItemsSource=null;
                                this.NoData.Visibility = Visibility.Hidden;
                                this.ListData.Visibility = Visibility.Visible;
                                // Processed data
                                foreach (ExchangeResponse data in this.exchangedatalist)
                                {
                                    ExchangeData edata = new ExchangeData();
                                    edata.excid = data.id;
                                    edata.deposit_address_amount_symbol = $"{data.deposit_address} {data.deposit_amount} {data.deposit_symbol}";
                                    edata.xels_address_amount = $"{data.xels_address} {data.xels_amount} XELS";

                                    if (data.status == 0)
                                    {
                                        edata.showstatus = "Wating for deposit";
                                    }
                                    else if (data.status == 1)
                                    {
                                        edata.showstatus = "Pending Exchange";
                                    }
                                    else if (data.status == 2)
                                    {
                                        edata.showstatus = "Complete";
                                    }

                                    this.exchangelist.Add(edata);
                                }

                                this.ExchangesList.ItemsSource = this.exchangelist;
                            }
                            else
                            {
                                this.ListData.Visibility = Visibility.Hidden;
                                this.NoData.Visibility = Visibility.Visible;
                            }
                            this.updatesuccess = true;
                        });
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message.ToString());
                        }

                    }

                }
        }

        public async Task LoadCreateAsync()
        {
            this.addr = await GetUnusedReceiveAddressesAsync(this.baseURL);
            this.addr = this.addr.TrimStart('"').TrimEnd('"');

        }

      
        private async Task<string> GetUnusedReceiveAddressesAsync(string path)
        {
            string getUrl = path + $"/wallet/unusedaddress?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
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

        public async Task DepositAsync(string symbol, double amount, ExchangeResponse exchangeResponse)
        {
            string addre = mywallet.Address;
            string privtKey= mywallet.PrivateKey;
            string  walltName= mywallet.Walletname;
            string coin = mywallet.Coin;
            string wathash = mywallet.Wallethash;

            StoredWallet mWallet = new StoredWallet();
            try
            {
                if (symbol == coin)
                {
                    mWallet.Address = addre;
                    mWallet.Walletname = walltName;
                    mWallet.Coin = coin;
                    mWallet.Wallethash = wathash;
                    mWallet.PrivateKey= Encryption.DecryptPrivateKey(privtKey);
                }
                //Initialize
                // Transfer
                var tx = await this.createWallet.TransferAsync(mWallet, exchangeResponse, amount);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }
     
        public async Task<ExchangeResponse> GetOrderAsync(string orderId)
        {
            ExchangeResponse exchangedata = new ExchangeResponse();
            try
            {
                string path = URLConfiguration.BaseURLExchange + "/api/getOrder/" + orderId;
                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(path);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    exchangedata = JsonConvert.DeserializeObject<ExchangeResponse>(content);

                    if (exchangedata.id == null)
                    {
                        MessageBox.Show($"Your provided Order Id: {orderId} is not found!");
                    }
                    else
                    {
                        await DepositAsync(exchangedata.deposit_symbol, exchangedata.deposit_amount, exchangedata);
                    }

                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }

                return exchangedata;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return exchangedata;
            }
        }
        private void DepositButton_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = this.ExchangesList;
            DataGridRow Row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            ExchangeData item = (ExchangeData) Row.DataContext;
            if (item != null)
            {
                Task<ExchangeResponse> task = Task.Run<ExchangeResponse>(async () => await GetOrderAsync(item.excid));
            }
            else
            {
                MessageBox.Show("Value Must be required!.");
            }

        }


        private void ExchangeOrderSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid())
            {
                if (this.mywallet == null || this.mywallet.PrivateKey == null)
                {
                    MessageBox.Show($"Your ethereum address is not imported properly. Please import your address again");
                }
                else
                {
                    ExchangeOrder exchangeOrder = new ExchangeOrder();
                    exchangeOrder.xels_address = this.addr;
                    exchangeOrder.deposit_amount = Convert.ToDouble(this.AmountTxt.Text);
                    exchangeOrder.deposit_symbol = this.SelectedCoin.Name;
                    exchangeOrder.user_code = this.mywallet.Wallethash;
                    Task.Run(async () => await NewOrderAsync(exchangeOrder));
                }
            }
        }


    }
}
