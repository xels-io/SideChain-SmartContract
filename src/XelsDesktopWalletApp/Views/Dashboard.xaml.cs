﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NBitcoin;
using Newtonsoft.Json;
using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.SmartContractView;
using XelsDesktopWalletApp.Views.layout;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
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
        private decimal confirmedBalance;
        private decimal unconfirmedBalance;
        private decimal spendableBalance;

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
        public decimal awaitingMaturity = 0;

        #endregion

        public Dashboard()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public Dashboard(string walletname)
        {
            InitializeComponent();

            //this.AccountComboBox.SelectedItem = this.walletName;
            this.DataContext = this;


            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            GetGeneralInfoAsync();

            LoadLoginAsync();

            GetHistoryAsync();

            if (URLConfiguration.Chain != "-sidechain")// (!this.sidechainEnabled)
            {
                _ = GetStakingInfoAsync(this.baseURL);
            }

            if (URLConfiguration.Chain == "-sidechain")// (!this.sidechainEnabled)
            {
                this.buttonPowMining.Visibility = Visibility.Hidden;
            }
            UpdateWalletAsync();
        }


        public async Task LoadLoginAsync()
        {
            await GetWalletBalanceAsync(this.baseURL);
        }

        private async Task GetWalletBalanceAsync(string path)
        {
            try
            {
                string getUrl = path + $"/wallet/balance?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();

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
                    this.NoData.Visibility = Visibility.Hidden;
                    this.HistoryList.Visibility = Visibility.Visible;

                    TransactionItemModel[] historyResponse = new TransactionItemModel[transactionsLen];
                   // historyResponse = this.historyModelArray.History[0].TransactionsHistory;

                   // GetTransactionInfo(historyResponse);
                }
                else
                {
                    this.HistoryList.Visibility = Visibility.Hidden;
                    this.NoData.Visibility = Visibility.Visible;
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }

        //private void GetTransactionInfo(TransactionItemModel[] transactions)
        //{

        //    foreach (TransactionItemModel transaction in transactions)
        //    {
        //        TransactionInfo transactionInfo = new TransactionInfo();

        //        //Type
        //        if (transaction.Type == TransactionItemType.Send)
        //        {
        //            transactionInfo.transactionType = "sent";
        //        }
        //        else if (transaction.Type == TransactionItemType.Received)
        //        {
        //            transactionInfo.transactionType = "received";
        //        }
        //        else if (transaction.Type == TransactionItemType.Staked)
        //        {
        //            transactionInfo.transactionType = "hybrid reward";
        //        }
        //        else if (transaction.Type == TransactionItemType.Mined)
        //        {
        //            transactionInfo.transactionType = "pow reward";
        //        }

        //        //Id
        //        transactionInfo.transactionId = transaction.Id;

        //        //Amount
        //        transactionInfo.transactionAmount = transaction.Amount ?? 0;

        //        //Fee
        //        if (transaction.Fee != null)
        //        {
        //            transactionInfo.transactionFee = transaction.Fee;
        //        }
        //        else
        //        {
        //            transactionInfo.transactionFee = 0;
        //        }

        //        //FinalAmount
        //        if (transactionInfo.transactionType != null)
        //        {
        //            if (transactionInfo.transactionType == "sent")
        //            {
        //                Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
        //                transactionInfo.transactionFinalAmount = $" - {finalAmt}";
        //            }
        //            else if (transactionInfo.transactionType == "received")
        //            {
        //                Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
        //                transactionInfo.transactionFinalAmount = $" + {finalAmt}";
        //            }
        //            else if (transactionInfo.transactionType == "hybrid reward")
        //            {
        //                Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
        //                transactionInfo.transactionFinalAmount = $" + {finalAmt}";
        //            }
        //            else if (transactionInfo.transactionType == "pow reward")
        //            {
        //                Money finalAmt = transactionInfo.transactionAmount + transactionInfo.transactionFee;
        //                transactionInfo.transactionFinalAmount = $" + {finalAmt}";
        //            }
        //        }
        //        //ConfirmedInBlock
        //        transactionInfo.transactionConfirmedInBlock = transaction.ConfirmedInBlock;
        //        if (transactionInfo.transactionConfirmedInBlock != 0 || transactionInfo.transactionConfirmedInBlock != null)
        //        {
        //            transactionInfo.transactionTypeName = TransactionItemTypeName.Confirmed;
        //        }
        //        else
        //        {
        //            transactionInfo.transactionTypeName = TransactionItemTypeName.Unconfirmed;
        //        }

        //        //Timestamp
        //        transactionInfo.transactionTimestamp = transaction.Timestamp;

        //        transactionInfo.transactionType = transactionInfo.transactionType.ToUpper();
        //        this.transactions.Add(transactionInfo);
        //        //this.transactions.Take(5);
        //    }

        //    this.HistoryList.ItemsSource = this.transactions.Take(5);

        //}

        public async Task GetGeneralInfoAsync()
        {
            await GetGeneralWalletInfoAsync(this.baseURL);
        }

        private async Task GetGeneralWalletInfoAsync(string path)
        {
            string getUrl = path + $"/wallet/general-info?Name={this.walletInfo.WalletName}";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
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
                    this.ParentSyncedTxt.Text = "syncing...";
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
                this.walletGeneralInfoModel.LastBlockSyncedHeight = this.walletGeneralInfoModel.LastBlockSyncedHeight ?? 0;
                this.LastBlockSyncedHeightTxt.Text = this.walletGeneralInfoModel.LastBlockSyncedHeight.ToString();
                this.ChainTipTxt.Text = this.walletGeneralInfoModel.ChainTip.ToString();

                this.ParentSyncedTxt.Text = this.percentSynced;

                this.ConnectionPercentTxt.Text = this.percentSynced;

                this.LastBlockSyncedHeightTxt.Text = this.walletGeneralInfoModel.LastBlockSyncedHeight.ToString();

                this.ChainTipTxt.Text = this.walletGeneralInfoModel.ChainTip.ToString();

                if (!this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Text = "Not Ok";
                }
                else if (this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Text = "Ok";
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
                this.stakingInfo.netStakeWeight = stakingInfoModel.netStakeWeight; //netStakingWeight
                this.awaitingMaturity = (this.unconfirmedBalance + this.confirmedBalance) - this.spendableBalance; //
                this.stakingInfo.expectedTime = stakingInfoModel.expectedTime; //expectedTime

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
                    this.ConnectionNotifyTxt.Text = "Not Ok";
                }
                else if (this.stakingEnabled && !this.sidechainEnabled)
                {
                    this.ConnectionNotifyTxt.Text = "Ok";
                }

                this.HybridWeightTxt.Text = $"{this.stakingInfo.weight} XELS";
                this.CoinsAwaitingMaturityTxt.Text = $"{this.awaitingMaturity} XELS";
                this.NetworkWeightTxt.Text = $"{this.stakingInfo.netStakeWeight} XELS";
                this.ExpectedRewardTimmeTxt.Text = this.stakingInfo.expectedTime.ToString();

            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }

        private async Task UpdateWalletAsync()
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
            this.Close();
        }
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Send send = new Send(this.walletName);
            send.Show();
            this.Close();
        }


        private void ImportAddrButton_Click(object sender, RoutedEventArgs e)
        {

            EthImport eImp = new EthImport(this.walletName);
            eImp.Show();
            this.Close();
        }

        private void StopPOWMiningButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionInfo item = (TransactionInfo)((sender as Button)?.Tag as ListViewItem)?.DataContext;

            TransactionDetail td = new TransactionDetail(this.walletName, item);
            td.Show();
            this.Close();
        }


        private void GotoHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            History history = new History(this.walletName);
            history.Show();
            this.Close();
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            //MyPopup.IsOpen = false;
        }


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

            SmartContractMain sc = new SmartContractMain(this.walletName);
            sc.Show();
            this.Close();
        }

        private void Hyperlink_NavigateAddressBook(object sender, RequestNavigateEventArgs e)
        {
            AddressBook ex = new AddressBook(this.walletName);
            ex.Show();
            this.Close();
        }
        private void Hyperlink_NavigateLogout(object sender, RequestNavigateEventArgs e)
        {
            LogoutConfirm lc = new LogoutConfirm(this.walletName);
            lc.Show();
            this.Close();
        }


        private void Hyperlink_NavigateAdvanced(object sender, RequestNavigateEventArgs e)
        {
            Advanced adv = new Advanced(this.walletName);
            adv.Show();
            this.Close();
        }

        private void Hyperlink_NavigateNewLayout(object sender, RequestNavigateEventArgs e)
        {
            //MainLayout history = new MainLayout();
            //history.Show();
            //this.Close();
            MainLayout mainLayout = new MainLayout(this.walletName);
            mainLayout.Show();
            this.Close();
        }


        //private void ImportAddrButton_Click(object sender, RoutedEventArgs e)
        //{

        //    EthImport eImp = new EthImport(this.walletName);
        //    eImp.Show();
        //    this.Close();
        //}

        //private void StopPOWMiningButton_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void DetailButton_Click(object sender, RoutedEventArgs e)
        //{
        //    TransactionInfo item = (TransactionInfo)((sender as Button)?.Tag as ListViewItem)?.DataContext;

        //    TransactionDetail td = new TransactionDetail(this.walletName, item);
        //    td.Show();
        //    this.Close();
        //}


        //private void GotoHistoryButton_Click(object sender, RoutedEventArgs e)
        //{
        //    History history = new History(this.walletName);
        //    history.Show();
        //    this.Close();
        //}
    }
}
