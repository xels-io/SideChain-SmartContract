using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Common;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Models.SmartContractModels;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {
        private DataGridPagination _cview;
        string baseURL = URLConfiguration.BaseURLMain;

        private string walletName = GlobalPropertyModel.WalletName;

        // Hisotory detail data
        private int? lastBlockSyncedHeight = 0;
        private int? confirmations = 0;
        public HistoryPage()
        {
            InitializeComponent();
            this.DataContext = this;

            _ = GetGeneralWalletInfoAsync();
            _ = GetWalletHistoryTimerAsync(this.baseURL);

        }

        #region api call

        private async Task GetGeneralWalletInfoAsync()
        {
            string getUrl = this.baseURL + $"/wallet/general-info?Name={this.walletName}";
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

        private async Task GetWalletHistoryTimerAsync(string path)
        {
            try
            {


                var content = "";
                List<TransactionItemModel> HistoryListForTimer = new List<TransactionItemModel>();

                ObservableCollection<TransactionItemModel> observableList = new ObservableCollection<TransactionItemModel>();

                string getUrl = path + $"/wallet/history?WalletName={this.walletName}&AccountName=account 0";

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
                        //this.HistoryListBinding.ItemsSource = observableList;

                        if (observableList.Count != 0)
                        {
                            this._cview = new DataGridPagination(observableList, 12);
                            this.HistoryListBinding.ItemsSource = this._cview;
                            PageCountWiseButton();
                        }
                        else
                        {
                            this.NoDataGrid.Visibility = Visibility.Visible;
                            this.HistoryDataGrid.Visibility = Visibility.Hidden;
                        }
                    }
                    catch (Exception e)
                    {

                        //throw;
                    }

                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        #endregion

        #region button events 
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionItemModel item = (TransactionItemModel)((sender as Button))?.DataContext;
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

        private void OnNextClicked(object sender, RoutedEventArgs e)
        {
            this._cview.MoveToNextPage();
        }

        private void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            this._cview.MoveToPreviousPage();
        }


        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _ = GetWalletHistoryTimerAsync(this.baseURL);
        }

        private void PageCountWiseButton()
        {
            int pageCount = this._cview.PageCount;
            List<Button> Buttons = new List<Button>();
            for (int i = 0; i < pageCount; i++)
            {
                // if(i < 3)
                {
                    Button button = new Button()
                    {
                        Content = string.Format($"{i + 1 }"),
                        Tag = i
                    };
                    Buttons.Add(button);
                   // button.Click += new RoutedEventHandler(button_Click);
                    //this.buttons.Children.Add(button);
                    this.buttons.ItemsSource = Buttons;

                    button.Click += new RoutedEventHandler(button_Click);
                }

            }
        }




        protected void button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int i = Convert.ToInt32(button.Content);
            // identify which button was clicked and perform necessary actions
            //MessageBox.Show($"Clicked + { i }");
            if (i > this._cview.CurrentPage)
            {
                this._cview.MoveToNextPageNumber(i);
            }
            if (i < this._cview.CurrentPage)
            {
                this._cview.MoveToPreviousPageNumber(i);
            }

        }

    }
}
