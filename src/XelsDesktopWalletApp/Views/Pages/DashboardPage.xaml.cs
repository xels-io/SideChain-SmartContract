﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NBitcoin;
using Newtonsoft.Json;
using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using System.Text;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
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

            if (URLConfiguration.Chain != "-sidechain")// (!this.sidechainEnabled)
            {
                _ = GetStakingInfoAsync(this.baseURL);
            }

            if (URLConfiguration.Chain == "-sidechain")// (!this.sidechainEnabled)
            {
                this.PowMiningButton.Visibility = Visibility.Hidden;
            }
            UpdateWalletAsync();
        }

        private string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";

        private WalletBalanceArray walletBalanceArray = new WalletBalanceArray();

        private HistoryModelArray historyModelArray = new HistoryModelArray();

        TransactionItemModelArray transactionItem = new TransactionItemModelArray();

        private List<TransactionInfo> transactions = new List<TransactionInfo>();

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

        #region Own Property

        public bool sidechainEnabled = false;
        public bool stakingEnabled = false;

        private bool hasBalance = false;
        private Money confirmedBalance;
        private Money unconfirmedBalance;
        private Money spendableBalance;

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

                    this.confirmedBalance = this.walletBalanceArray.Balances[0].AmountConfirmed;
                    this.unconfirmedBalance = this.walletBalanceArray.Balances[0].AmountUnconfirmed;
                    this.spendableBalance = this.walletBalanceArray.Balances[0].SpendableAmount;

                    if ((this.confirmedBalance + this.unconfirmedBalance) > 0)
                    {
                        this.hasBalance = true;
                    }
                    else
                    {
                        this.hasBalance = false;
                    }
                    // Populate - Balance info
                    this.ConfirmedBalanceTxt.Text = $"{this.confirmedBalance} XELS";
                    this.UnconfirmedBalanceTxt.Text = $"{this.unconfirmedBalance} (unconfirmed)";
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
            await GetWalletHistoryAsync(this.baseURL);
        }

        private async Task GetWalletHistoryAsync(string path)
        {
            string getUrl = path + $"/wallet/history?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();

                this.transactionItem = JsonConvert.DeserializeObject<TransactionItemModelArray>(content);

                if (this.transactionItem.Transactions != null && this.transactionItem.Transactions.Length > 0)
                {
                    int transactionsLen = this.transactionItem.Transactions.Length;
                    //this.NoData.Visibility = Visibility.Hidden;
                    this.HistoryList.Visibility = Visibility.Visible;

                    TransactionItemModel[] historyResponse = new TransactionItemModel[transactionsLen];
                    historyResponse = this.historyModelArray.History[0].TransactionsHistory;

                    GetTransactionInfo(historyResponse);
                }
                else
                {
                    this.HistoryList.Visibility = Visibility.Hidden;
                    //this.NoData.Visibility = Visibility.Visible;
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }

        private void GetTransactionInfo(TransactionItemModel[] transactions)
        {

            foreach (TransactionItemModel transaction in transactions)
            {
                TransactionInfo transactionInfo = new TransactionInfo();

                //Type
                if (transaction.Type == TransactionItemType.Send)
                {
                    transactionInfo.transactionType = "sent";
                }
                else if (transaction.Type == TransactionItemType.Received)
                {
                    transactionInfo.transactionType = "received";
                }
                else if (transaction.Type == TransactionItemType.Staked)
                {
                    transactionInfo.transactionType = "hybrid reward";
                }
                else if (transaction.Type == TransactionItemType.Mined)
                {
                    transactionInfo.transactionType = "pow reward";
                }

                //Id
                transactionInfo.transactionId = transaction.Id;

                //Amount
                transactionInfo.transactionAmount = transaction.Amount ?? 0;

                //Fee
                if (transaction.Fee != null)
                {
                    transactionInfo.transactionFee = transaction.Fee;
                }
                else
                {
                    transactionInfo.transactionFee = 0;
                }

                //FinalAmount
                if (transactionInfo.transactionType != null)
                {
                    if (transactionInfo.transactionType == "sent")
                    {
                        Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
                        transactionInfo.transactionFinalAmount = $" - {finalAmt}";
                    }
                    else if (transactionInfo.transactionType == "received")
                    {
                        Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
                        transactionInfo.transactionFinalAmount = $" + {finalAmt}";
                    }
                    else if (transactionInfo.transactionType == "hybrid reward")
                    {
                        Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
                        transactionInfo.transactionFinalAmount = $" + {finalAmt}";
                    }
                    else if (transactionInfo.transactionType == "pow reward")
                    {
                        Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
                        transactionInfo.transactionFinalAmount = $" + {finalAmt}";
                    }
                }
                //ConfirmedInBlock
                transactionInfo.transactionConfirmedInBlock = transaction.ConfirmedInBlock;
                if (transactionInfo.transactionConfirmedInBlock != 0 || transactionInfo.transactionConfirmedInBlock != null)
                {
                    transactionInfo.transactionTypeName = TransactionItemTypeName.Confirmed;
                }
                else
                {
                    transactionInfo.transactionTypeName = TransactionItemTypeName.Unconfirmed;
                }

                //Timestamp
                transactionInfo.transactionTimestamp = transaction.Timestamp;

                transactionInfo.transactionType = transactionInfo.transactionType.ToUpper();
                this.transactions.Add(transactionInfo);
                //this.transactions.Take(5);
            }

            this.HistoryList.ItemsSource = this.transactions.Take(5);

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

                this.processedText = $"Processed { this.walletGeneralInfoModel.LastBlockSyncedHeight ?? 0} out of { this.walletGeneralInfoModel.ChainTip} blocks.";
                this.blockChainStatus = $"Synchronizing.  { this.processedText}";

                if (this.walletGeneralInfoModel.ConnectedNodes == 1)
                {
                    this.connectedNodesStatus = "1 connection";
                    this.ConnectionStatusTxt.Text = this.connectedNodesStatus;
                    this.ConnectedCountTxt.Text = this.connectedNodesStatus;
                }
                else
                if (this.walletGeneralInfoModel.ConnectedNodes >= 0)
                {
                    this.ConnectionStatusTxt.Text = $"{ this.walletGeneralInfoModel.ConnectedNodes} connections";
                    this.ConnectedCountTxt.Text = $"{ this.walletGeneralInfoModel.ConnectedNodes} connections";
                }

                if (!this.walletGeneralInfoModel.IsChainSynced)
                {
                    this.ParentSyncedTxt.Text = "syncing...".ToString();
                }
                else
                {
                    this.percentSyncedNumber = ((this.walletGeneralInfoModel.LastBlockSyncedHeight / this.walletGeneralInfoModel.ChainTip) * 100) ?? 0;
                    if (Math.Round(this.percentSyncedNumber) == 100 && this.walletGeneralInfoModel.LastBlockSyncedHeight != this.walletGeneralInfoModel.ChainTip)
                    {
                        this.ParentSyncedTxt.Text = "99";
                    }

                    this.percentSynced = $"{ Math.Round(this.percentSyncedNumber)} %";

                    if (this.percentSynced == "100%")
                    {
                        this.blockChainStatus = $"Up to date.  { this.processedText}";
                    }
                }

                // populate
                this.LastBlockSyncedHeightTxt.Text = this.walletGeneralInfoModel.LastBlockSyncedHeight.ToString() ?? "0" ;
                
                this.ChainTipTxt.Text = this.walletGeneralInfoModel.ChainTip.ToString();

                this.ParentSyncedTxt.Text = this.percentSynced;

                this.ConnectionPercentTxt.Text = this.percentSynced;

                if (!this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Content = "Not Staking";
                }
                else if (this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Content = "Staking";
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
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

                this.stakingInfo.enabled = stakingInfoModel.enabled; //stakingEnabled
                this.stakingEnabled = this.stakingInfo.enabled;
                this.stakingInfo.staking = stakingInfoModel.staking; //stakingActive

                this.stakingInfo.weight = stakingInfoModel.weight; //stakingWeight
                this.HybridWeightTxt.Text = $"{this.stakingInfo.weight} XELS";

                this.NetworkWeightTxt.Text = $"{stakingInfoModel.netStakeWeight.ToString()} xlc"; //netStakingWeight
                
                this.CoinsAwaitingMaturityTxt.Text = $"{((this.unconfirmedBalance + this.confirmedBalance) - this.spendableBalance).ToString()}xlc"; //

                this.ExpectedRewardTimmeTxt.Text = stakingInfoModel.expectedTime.ToString(); //expectedTime
                 
                if (this.stakingInfo.staking)
                {
                    this.isStarting = false;
                }
                else
                {
                    this.isStopping = false;
                }

                //populate

                if (!this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Content = "Not Staking";
                }
                else if (this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Content = "Staking";
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
                        this.sels = await this.createWallet.GetBalanceAsync(this.selswallet.Address, "SELS");
                    }
                }
                if (this.belswallet != null)
                {
                    if (this.belswallet.Address != null)
                    {
                        this.bels = await this.createWallet.GetBalanceAsync(this.belswallet.Address, "BELS");
                    }
                }
                // Populate coins
                this.SelsCoinTxt.Text = this.sels + " SELS";
                this.BelsCoinTxt.Text = this.bels + " BELS";

            }
        }

        private void receiveButton_Click(object sender, RoutedEventArgs e)
        {
            Receive receive = new Receive(this.walletName);
            receive.Show();
            
        }
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Send send = new Send(this.walletName);
            send.Show();
           
        }

        private void ImportAddrButton_Click(object sender, RoutedEventArgs e)
        {

            EthImport eImp = new EthImport(this.walletName);
            eImp.Show();           
        }

        private async void StartPOWMiningButton_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = this.baseURL + $"/mining/startmining"; ///mining/stopmining

            HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl,this.walletInfo );

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                this.PowMiningButton.Visibility = Visibility.Hidden;
                this.StopPowMiningButton.Visibility = Visibility.Visible;
            }
        }

        private async void StopPOWMiningButton_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = this.baseURL + $"/mining/stopmining";

            HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, true);

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                this.PowMiningButton.Visibility = Visibility.Visible;
                this.StopPowMiningButton.Visibility = Visibility.Hidden;
            }
        }

        private async void StartHybridMiningButton_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = this.baseURL + $"/staking/startstaking";

            HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, this.walletName);

            string content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                this.HybridMinginButton.Content = "Stop Hybrid Mining";
            }
            this.HybridMinginButton.Content = "Stop Hybrid Mining";
        }

        private async void StopHybridMiningButton_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = this.baseURL + $"/staking/stopstaking";

            HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(apiUrl, true);

            string content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                this.HybridMinginButton.Content = "Stop Hybrid Mining";
            }
            
        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionInfo item = (TransactionInfo)((sender as Button)?.Tag as ListViewItem)?.DataContext;

            TransactionDetail td = new TransactionDetail(this.walletName, item);
            td.Show();
            
        }


        private void GotoHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            History history = new History(this.walletName);
            history.Show();
            
        }
    }
}