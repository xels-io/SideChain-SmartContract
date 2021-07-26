﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NBitcoin;
using Newtonsoft.Json;
using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.Pages.Modals;
using XelsDesktopWalletApp.Models.SmartContractModels;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using XelsDesktopWalletApp.Views.layout;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {

        private string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";

        private WalletBalanceArray walletBalanceArray = new WalletBalanceArray();


        private CreateWallet createWallet = new CreateWallet();

        private StoredWallet selswallet = new StoredWallet();

        private StoredWallet belswallet = new StoredWallet();

        private string sels = "";

        private string bels = "";

        private readonly WalletInfo walletInfo = new WalletInfo();

        private string walletName;

        public string WalletName
        {
            get => this.walletName;
            set
            {
                this.walletName = value;
            }
        }
        // Hisotory detail data
        private int? lastBlockSyncedHeight = 0;
        private int? confirmations = 0;

        #region Own Property

        public bool sidechainEnabled = false;
        public bool stakingEnabled = false;

        private bool hasBalance;
        private double confirmedBalance;
        private double unconfirmedBalance;
        private double spendableBalance;

        private string percentSynced;

        // general info
        private WalletGeneralInfoModel walletGeneralInfoModel = new WalletGeneralInfoModel();
        private string processedText;
        private string blockChainStatus;
        private string connectedNodesStatus;
        private double percentSyncedNumber = 0;

        // Staking  Info
        public bool isStarting = false;
        public bool isStopping = false;

        private StakingInfoModel stakingInfo = new StakingInfoModel();
        public Money awaitingMaturity = 0;

       public  DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public DashboardPage()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public DashboardPage(string walletname)
        {
            InitializeComponent();

            //this.AccountComboBox.SelectedItem = this.walletName;
            this.DataContext = this;


            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            GetGeneralWalletInfoAsync();

            GetWalletBalanceAsync();

            GetHistoryAsync();
            GetMaxBalanceAsync();

            //if (GlobalPropertyModel.HasBalance && URLConfiguration.Chain != "-sidechain")// (!this.sidechainEnabled)
            //{
            //    _ = GetStakingInfoAsync(this.baseURL);
            //}

            if (URLConfiguration.Chain == "-sidechain")// (!this.sidechainEnabled)
            {
                this.PowMiningButton.Visibility = Visibility.Hidden;
               
            }

            if(GlobalPropertyModel.MiningStart == true)
            {
                this.StopPowMiningButton.Visibility = Visibility.Visible;
            }

            if (GlobalPropertyModel.StakingStart == true)
            {
                this.StopHybridMinginButton.Visibility = Visibility.Visible;
                this.UnlockGrid.Visibility = Visibility.Hidden;
                this.MiningInfoBorder.Visibility = Visibility.Visible;
                this.t.Visibility = Visibility.Hidden;
                this.StakingInfo.Content = "Staking";
            }

            UpdateWalletAsync();
        }

        #endregion

        private async Task GetWalletBalanceAsync()
        {
            try
            {
                string getUrl = this.baseURL + $"/wallet/balance?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
                content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    this.walletBalanceArray = JsonConvert.DeserializeObject<WalletBalanceArray>(content);
                    //double val = Convert.ToDouble(this.ConfirmedBalanceTxt.Text) + 1;
                    //this.ConfirmedBalanceTxt.Text = val.ToString();
                    this.ConfirmedBalanceTxt.Text = $"{(this.walletBalanceArray.Balances[0].AmountConfirmed / 100000000).ToString("0.##############")} {GlobalPropertyModel.CoinUnit}";
                    this.UnconfirmedBalanceTxt.Text = $"{(this.walletBalanceArray.Balances[0].AmountUnconfirmed / 100000000).ToString("0.##############")} (unconfirmed)";

                    this.spendableBalance = (this.walletBalanceArray.Balances[0].SpendableAmount/100000000);

                    if ((this.walletBalanceArray.Balances[0].AmountConfirmed + this.walletBalanceArray.Balances[0].AmountUnconfirmed) > 0)
                    {
                        GlobalPropertyModel.HasBalance = true;

                        if (GlobalPropertyModel.HasBalance && URLConfiguration.Chain != "-sidechain")// (!this.sidechainEnabled)
                        {
                            _ = GetStakingInfoAsync(this.baseURL);
                        }
                    }
                    else
                    {
                        GlobalPropertyModel.HasBalance = false;
                        this.HybridMiningInfoBorder.Visibility = Visibility.Hidden;
                    }
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

        public async Task GetHistoryAsync()
        {
            //await GetWalletHistoryAsync(this.baseURL);
             await GetWalletHistoryTimerAsync(this.baseURL);
        }

        private async Task GetWalletHistoryAsync(string path)
        {
            string getUrl = path + $"/wallet/history?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {

                try
                {
                    var history = JsonConvert.DeserializeObject<HistoryModelArray>(content);

                    foreach (var h in history.History)
                    {
                        //for(int i =0; i< h.TransactionsHistory.Count;i++)
                        //if(i <= 5)
                        //{
                            this.HistoryListBinding.ItemsSource = h.TransactionsHistory;
                        //}                        
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

        private async Task GetWalletHistoryTimerAsync(string path)
        {
            var content = "";
            List<TransactionItemModel> HistoryListForTimer = new List<TransactionItemModel>();
            ObservableCollection<TransactionItemModel> observableList = new ObservableCollection<TransactionItemModel>();
            string getUrl = path + $"/wallet/history?WalletName={this.walletInfo.WalletName}&AccountName=account 0";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var history = JsonConvert.DeserializeObject<HistoryModelArray>(content);

                    foreach (var h in history.History)
                    {
                        HistoryListForTimer.AddRange(h.TransactionsHistory);
                    }
                    observableList = new ObservableCollection<TransactionItemModel>(HistoryListForTimer);
                    this.HistoryListBinding.ItemsSource = observableList;
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

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionItemModel item = (TransactionItemModel)((sender as Button)?.Tag as ListViewItem)?.DataContext;
            this.DetailsPopup.IsOpen = true;

            this.TypeTxt.Text = item.Type;
            this.TotalAmountTxt.Text = item.Amount.ToString();
            this.AmountSentTxt.Text = item.Amount.ToString();
            this.FeeTxt.Text = item.Fee.ToString();
            this.DateTxt.Text = item.Timestamp.ToString();
            this.BlockTxt.Text = item.ConfirmedInBlock.ToString();

            if (item.ConfirmedInBlock != 0 || item.ConfirmedInBlock != null)
            {
                this.confirmations = this.lastBlockSyncedHeight - item.ConfirmedInBlock + 1; 
            }
            else
            {
                this.confirmations = 0;
            }
            this.ConfirmationsTxt.Text = this.confirmations.ToString();

            this.TransactionIDTxt.Text = item.Id.ToString();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.TransactionIDTxt.Text);
           // MessageBox.Show("Copied Successfully :- " + this.TransactionIDTxt.Text.ToString());
        }

        private void HidePopup_Click(object sender, RoutedEventArgs e)
        {
            this.DetailsPopup.IsOpen = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DetailsPopup.IsOpen = false;
        }

        private async Task GetGeneralWalletInfoAsync()
        {
            string getUrl = this.baseURL + $"/wallet/general-info?Name={this.walletInfo.WalletName}";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
            content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {

                this.walletGeneralInfoModel = JsonConvert.DeserializeObject<WalletGeneralInfoModel>(content);

               // this.lastBlockSyncedHeight = this.walletGeneralInfoModel.LastBlockSyncedHeight; // for history data

                this.processedText = $"Processed { this.walletGeneralInfoModel.LastBlockSyncedHeight ?? 0} out of { this.walletGeneralInfoModel.ChainTip} blocks.";
                this.blockChainStatus = $"Synchronizing.  { this.processedText}";

                this.ConnectionStatusTxt.Text = $"{ this.walletGeneralInfoModel.ConnectedNodes} connection";
                this.ConnectedCountTxt.Text =  this.walletGeneralInfoModel.ConnectedNodes.ToString();

                if (!this.walletGeneralInfoModel.IsChainSynced)
                {
                    this.ConnectionPercentTxt.Text =  this.ParcentSyncedTxt.Text = "syncing...".ToString();
                    
                }
                else
                {
                    this.percentSyncedNumber = ((this.walletGeneralInfoModel.LastBlockSyncedHeight / this.walletGeneralInfoModel.ChainTip) * 100) ?? 0;
                    
                    if (Math.Round(this.percentSyncedNumber) == 100 && this.walletGeneralInfoModel.LastBlockSyncedHeight != this.walletGeneralInfoModel.ChainTip)
                    {
                        this.ParcentSyncedTxt.Text = "99";
                    }

                    this.percentSynced = $"{ Math.Round(this.percentSyncedNumber)} %";

                    if (this.percentSynced == "100%")
                    {
                        this.blockChainStatus = $"Up to date.  { this.processedText}";
                    }
                }

                // populate
                this.LastBlockSyncedHeightTxt.Text = this.walletGeneralInfoModel.LastBlockSyncedHeight.ToString() ?? "0";

                this.ChainTipTxt.Text = this.walletGeneralInfoModel.ChainTip.ToString();

       

                if (!GlobalPropertyModel.StakingStart && !this.sidechainEnabled)
                {
                    this.StakingInfo.Content = "Not Staking";
                    //try
                    //{
                    //    this.StakingInfoImage.Source = new BitmapImage(new Uri("/Assets/Images/thumbsdown.png", UriKind.Relative));

                    //    var uriSource = new Uri("/Assets/Images/thumbsdown.png");
                    //    this.StakingInfoImage.Source = new BitmapImage(uriSource);
                    //}
                    //catch (Exception e)
                    //{

                    //    throw;
                    //}

                }
                else if (this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.StakingInfo.Content = "Staking";
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }

        private async Task GetMaxBalanceAsync()
        {
            var content = "";

            MaximumBalance maximumBalance = new MaximumBalance();
            maximumBalance.WalletName = this.walletInfo.WalletName;
            // maximumBalance.AccountName = "account 0";
            maximumBalance.FeeType = "medium";
            maximumBalance.AllowUnconfirmed = true;

            try
            {
                string postUrl = this.baseURL +
                $"/wallet/maximumbalancewpf?WalletName={maximumBalance.WalletName}&FeeType={maximumBalance.FeeType}&AllowUnconfirmed={maximumBalance.AllowUnconfirmed}";
                //&accountName={maximumBalance.AccountName}
                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(postUrl);

                content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var balance = JsonConvert.DeserializeObject<WalletBalance>(content);


                    GlobalPropertyModel.SpendableBalance = (balance.MaxSpendableAmount / 100000000);
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

        private async Task GetStakingInfoAsync(string path)
        {
            string getUrl = path + $"/staking/getstakinginfo";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            if (response.IsSuccessStatusCode)
            {
                StakingInfoModel stakingInfoModel = new StakingInfoModel();
                content = await response.Content.ReadAsStringAsync();

                stakingInfoModel = JsonConvert.DeserializeObject<StakingInfoModel>(content);
                               
                this.HybridWeightTxt.Text = $"{this.stakingInfo.weight}{" "} {GlobalPropertyModel.CoinUnit}";

                this.NetworkWeightTxt.Text = $"{stakingInfoModel.netStakeWeight.ToString()} {" "} {GlobalPropertyModel.CoinUnit}"; //netStakingWeight

                var balance = ((this.walletBalanceArray.Balances[0].AmountConfirmed / 100000000) + (this.walletBalanceArray.Balances[0].AmountUnconfirmed / 100000000));

            this.CoinsAwaitingMaturityTxt.Text = $"{(balance - GlobalPropertyModel.SpendableBalance).ToString("0.##############")} {" "} {GlobalPropertyModel.CoinUnit}"; //

                this.ExpectedRewardTimmeTxt.Text = stakingInfoModel.expectedTime.ToString(); //expectedTime

                if (stakingInfoModel.enabled && URLConfiguration.Chain != "-sidechain")
                {
                    this.isStarting = false;                  
                    this.MiningInfoBorder.Visibility = Visibility.Visible;
                    this.t.Visibility = Visibility.Hidden;
                    this.StakingInfo.Content = "Staking";
                }
                else
                {
                    this.isStopping = false;                   
                    this.StakingInfo.Content = "Not Staking";
                    //this.StakingInfoImage.Source;
                }                
            }
            else
            {
                MessageBox.Show($"Error Code{response.StatusCode} : Message - {response.ReasonPhrase}");
            }
        }

        private async void UpdateWalletAsync()
        {
            //this.createWallet.Initialize("SELS");
            //this.createWallet.Initialize("BELS");

            string walletCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walletFile = Path.Combine(walletCurrentDirectory, @"..\..\..\File\Wallets.json");
            string path = Path.GetFullPath(walletFile);

            if (File.Exists(path))
            {
                this.selswallet = this.createWallet.GetLocalWallet(this.walletName, "SELS");
                this.belswallet = this.createWallet.GetLocalWallet(this.walletName, "BELS");

                if (this.selswallet != null)
                {
                    if (this.selswallet.Address != null)
                    {
                        //this.sels = await this.createWallet.GetBalanceAsync(this.selswallet.Address, "SELS"); 
                        // found error
                    }
                }
                if (this.belswallet != null)
                {
                    if (this.belswallet.Address != null)
                    {
                        //this.bels = await this.createWallet.GetBalanceAsync(this.belswallet.Address, "BELS");
                        // found error
                    }
                }
                // Populate coins
                this.SelsCoinTxt.Text = this.sels + " SELS";
                this.BelsCoinTxt.Text = this.bels + " BELS";

            }
        }

        private void ReceiveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dashboard.Children.Add(new ReceiveUserControl(this.walletName));

            //Receive receive = new Receive(this.walletName);
            //receive.ShowDialog();

        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //Send send = new Send(this.walletName);
            //send.Show();

             this.Dashboard.Children.Add(new SendUserControl(this.walletName));

        }

        private void ImportAddrButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dashboard.Children.Add(new ImportSelsBelsUserControl(this.walletName));
            //EthImport eImp = new EthImport(this.walletName);
            //eImp.Show();
        }

        private async void StartPOWMiningButton_Click(object sender, RoutedEventArgs e)
        {

            string apiUrl = this.baseURL + $"/mining/startmining"; ///mining/stopmining

            HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, this.walletInfo);

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                this.PowMiningButton.Visibility = Visibility.Hidden;
                this.StopPowMiningButton.Visibility = Visibility.Visible;
                GlobalPropertyModel.MiningStart = true;
            }
        }

        private async void StopPOWMiningButton_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = this.baseURL + $"/mining/stopmining";

            try
            {
                HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, true);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    this.PowMiningButton.Visibility = Visibility.Visible;
                    this.StopPowMiningButton.Visibility = Visibility.Hidden;
                    GlobalPropertyModel.MiningStart = false;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            

            
        }

        private async void StartHybridMiningButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.Password.Password)) {
                WalletLoadRequest UserWallet = new WalletLoadRequest()
                {
                    Name = this.walletName,
                    Password = this.Password.Password,
                };

                string apiUrl = this.baseURL + $"/staking/startstaking";

                HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, UserWallet);

                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    this.UnlockGrid.Visibility = Visibility.Hidden;
                    this.StopHybridMinginButton.Visibility = Visibility.Visible;
                    this.Password.Password = "";
                    this.MiningInfoBorder.Visibility = Visibility.Visible;
                    this.t.Visibility = Visibility.Hidden;
                    GlobalPropertyModel.StakingStart = true;

                    this.StakingInfo.Content = "Staking";
                }
                else
                {
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach (var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);
                        
                    }
                    this.Password.Password = "";
                }
            }
            else
            {
                MessageBox.Show("Please, enter password to unlock");
                
            }           

        }

        private async void StopHybridMiningButton_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = this.baseURL + $"/staking/stopstaking";

            HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, true);

            string content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                this.UnlockGrid.Visibility = Visibility.Visible;
                this.StopHybridMinginButton.Visibility = Visibility.Hidden;
                this.MiningInfoBorder.Visibility = Visibility.Hidden;
                this.t.Visibility = Visibility.Visible;
                GlobalPropertyModel.StakingStart = false;
                this.StakingInfo.Content = "Not Staking";
            }
        }

        private void GotoHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HistoryPage(this.walletName));            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //DispatcherTimer dispatcherTimer = new DispatcherTimer();
            URLConfiguration.Pagenavigation = false;
            this.dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            //if (!URLConfiguration.Pagenavigation)
           // {
                this.dispatcherTimer.Start();
            //}

        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //if (!URLConfiguration.Pagenavigation)
            //{
                try
                {
                    this.DataContext = this;
                    this.walletName = this.walletName;
                    this.walletInfo.WalletName = this.walletName;
                    _ = GetGeneralWalletInfoAsync();
                    _ = GetWalletHistoryTimerAsync(this.baseURL);
                    _ = GetWalletBalanceAsync();
                    _ = GetMaxBalanceAsync();
                    UpdateWalletAsync();

                    if (URLConfiguration.Chain == "-sidechain")// (!this.sidechainEnabled)
                    {
                        this.PowMiningButton.Visibility = Visibility.Hidden;

                    }

                    if (GlobalPropertyModel.MiningStart == true)
                    {
                        this.StopPowMiningButton.Visibility = Visibility.Visible;
                    }

                    if (GlobalPropertyModel.StakingStart == true)
                    {
                        this.StopHybridMinginButton.Visibility = Visibility.Visible;
                        this.UnlockGrid.Visibility = Visibility.Hidden;
                        this.MiningInfoBorder.Visibility = Visibility.Visible;
                        this.t.Visibility = Visibility.Hidden;
                        this.StakingInfo.Content = "Staking";
                    }
                }
                catch (Exception a)
                {
                    string exMessage = a.Message.ToString();
                    throw;
                }
            //}
            //else
            //{
            //    this.dispatcherTimer.Stop();
            //}
          
           
        }
    }
}
