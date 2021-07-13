using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
         
        public HistoryPage(string walletname)
        {
            InitializeComponent();            
            //this.HistoryListBinding.Visibility = Visibility.Hidden;
           // this.NoData.Visibility = Visibility.Visible;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            _ = GetWalletHistoryAsync(this.baseURL);
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

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionInfo item = (TransactionInfo)((sender as Button)?.Tag as ListViewItem)?.DataContext;

            TransactionDetail td = new TransactionDetail(this.walletName, item);
            td.Show();

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
