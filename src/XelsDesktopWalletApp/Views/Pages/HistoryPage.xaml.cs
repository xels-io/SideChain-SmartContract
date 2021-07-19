using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Base
        string baseURL = URLConfiguration.BaseURLMain;// "http://localhost:37221/api";
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

        private HistoryModelArray historyModelArray = new HistoryModelArray();
        private List<TransactionInfo> transactions = new List<TransactionInfo>();

        // Hisotory detail data
        private int? lastBlockSyncedHeight = 0;
        private int? confirmations = 0;

        public HistoryPage(string walletname)
        {
            InitializeComponent();            
            //this.HistoryListBinding.Visibility = Visibility.Hidden;
           // this.NoData.Visibility = Visibility.Visible;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            _= GetGeneralWalletInfoAsync();
           // _ = GetWalletHistoryAsync(this.baseURL);
            _ = GetWalletHistoryTimerAsync(this.baseURL);
        }

        private async Task GetGeneralWalletInfoAsync()
        {
            string getUrl = this.baseURL + $"/wallet/general-info?Name={this.walletInfo.WalletName}";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
            content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var wallet = JsonConvert.DeserializeObject<WalletGeneralInfoModel>(content);
                this.lastBlockSyncedHeight = wallet.LastBlockSyncedHeight; // for history detail data
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
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

                    foreach(var h in history.History)
                    {
                        this.HistoryListBinding.ItemsSource = h.TransactionsHistory;
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
            //System.Windows.MessageBox.Show("Copied Successfully :- " + this.TransactionIDTxt.Text.ToString());
        }

        private void HidePopup_Click(object sender, RoutedEventArgs e)
        {
            this.DetailsPopup.IsOpen = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DetailsPopup.IsOpen = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _ = GetWalletHistoryTimerAsync(this.baseURL);
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
            //    }

            //    this.HistoryListBinding.ItemsSource = this.transactions;
            //}

        }
}
