using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using NBitcoin;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Window
    {

        #region Base
        //static HttpClient client = new HttpClient();
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
        public History()
        {
            InitializeComponent();
        }
        public History(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;
            this.HistoryListBinding.Visibility = Visibility.Hidden;
            this.NoData.Visibility = Visibility.Visible;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            _ = GetWalletHistoryAsync(this.baseURL);
            AddtoHistoryList();
        }

        private async Task GetWalletHistoryAsync(string path)
        {
            string getUrl = path + $"/wallet/history?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);


            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
                
                this.historyModelArray = JsonConvert.DeserializeObject<HistoryModelArray>(content);

                //foreach (var h in this.historyModelArray.History)
                //{
                //    this.HistoryListBinding.ItemsSource = h.TransactionsHistory;
                //}

                //if (this.historyModelArray.History != null && this.historyModelArray.History[0].TransactionsHistory.Length > 0)
                //{
                //    int transactionsLen = this.historyModelArray.History[0].TransactionsHistory.Length;
                //    this.NoData.Visibility = Visibility.Hidden;
                //    this.HistoryListBinding.Visibility = Visibility.Visible;

                //    TransactionItemModel[] historyResponse = new TransactionItemModel[transactionsLen];
                //   // historyResponse = this.historyModelArray.History[0].TransactionsHistory;

                //    GetTransactionInfo(historyResponse);
                //}
                //else
                //{
                //    this.HistoryListBinding.Visibility = Visibility.Hidden;
                //    this.NoData.Visibility = Visibility.Visible;
                //}
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
        //    }

        //    this.HistoryListBinding.ItemsSource = this.transactions;
        //}


        private void AddtoHistoryList()
        {
            List<TransactionItemModel> list = new List<TransactionItemModel>();
            List<PaymentDetailModel> paymentlist = new List<PaymentDetailModel>();

            TransactionItemModel data1 = new TransactionItemModel();
            data1.Type = "mined";
            data1.ToAddress = "XHZn7EbwkdqG1cmdVajD8H1UvzgLHXBAdE";
            data1.Id = "a18cad8faedacf5768157013e45a3331c604955a4735a7c01f6dd1edbfdbcbd8";
            data1.Amount = 5000000000;
            data1.Payments = paymentlist;
            data1.ConfirmedInBlock = 6627;
            data1.Timestamp = "1624276245";
            data1.TxOutputTime = 1624276245;
            data1.TxOutputIndex = 0;
            list.Add(data1);

            TransactionItemModel data2 = new TransactionItemModel();
            data2.Type = "mined";
            data2.ToAddress = "XHZn7EbwkdqG1cmdVajD8H1UvzgLHXBAdE";
            data2.Id = "ef55855a89553405dcf6c26317f4ed189d728bd1c970c40b7469559086f30709";
            data2.Amount = 5000000000;
            data2.Payments = paymentlist;
            data2.ConfirmedInBlock = 6625;
            data2.Timestamp = "1624276144";
            data2.TxOutputTime = 1624276144;
            data2.TxOutputIndex = 0;
            list.Add(data2);

            this.HistoryListBinding.ItemsSource = list;


            this.HistoryListBinding.Visibility = Visibility.Visible;
            this.NoData.Visibility = Visibility.Hidden;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionInfo item = (TransactionInfo)((sender as Button)?.Tag as ListViewItem)?.DataContext;

            this.DetailsPopup.IsOpen = true;
            //TransactionDetail td = new TransactionDetail(this.walletName, item);
            //td.Show();

        }

        private void HidePopup_Click(object sender, RoutedEventArgs e)
        {
            this.DetailsPopup.IsOpen = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DetailsPopup.IsOpen = false;
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


    }
}
