﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Exchange.xaml
    /// </summary>
    public partial class Exchange : Window
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

        private StoredWallet mywallet = new StoredWallet();
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

        public Exchange()
        {
            InitializeComponent();
        }
        public Exchange(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.walletName = this.walletName;
            this.mywallet = this.createWallet.GetLocalWalletDetails(this.walletInfo.walletName);

            LoadCreate();
            UpdateExchangeListAsync();
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
                
                HttpResponseMessage response = await client.PostAsJsonAsync(postUrl, code);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
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

        public async Task<ExchangeResponse> GetOrderAsync(string orderId)
        {
            try
            {
                string path = URLConfiguration.BaseURLExchange + "/api/getOrder/" + orderId;
                var content = "";
                ExchangeResponse exchangedata = new ExchangeResponse();

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    exchangedata = JsonConvert.DeserializeObject<ExchangeResponse>(content);

                    if (exchangedata.id != null)
                    {
                        MessageBox.Show($"Your provided Order Id: {orderId} is not found!");
                    }
                    else
                    {
                        //Deposit(exchangedata.deposit_symbol, exchangedata.deposit_amount, exchangedata.deposit_address);
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
                throw;
            }
        }

        //public void NewOrder(string data)
        //{
        //    try
        //    {
        //        string path = URLConfiguration.BaseURLExchange + "/api/new-order";

        //        WebRequest requestObjPost = WebRequest.Create(path);
        //        requestObjPost.Headers.Add("Authorization", "1234567890");
        //        requestObjPost.Method = "POST";
        //        requestObjPost.ContentType = "application/x-www-form-urlencoded";

        //        string postdata = "{" + data + "}";

        //        using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
        //        {
        //            streamWriter.WriteLine(postdata);
        //            streamWriter.Flush();
        //            streamWriter.Close();

        //            var httpResponse = (HttpWebResponse)requestObjPost.GetResponse();

        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                var result = streamReader.ReadToEnd();
        //                streamReader.Close();
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}
        #endregion

        public async Task UpdateExchangeListAsync()
        {
            if (this.mywallet.Wallethash != null || this.mywallet.Wallethash != "")
            {
                this.exchangedatalist = await GetOrdersAsync(this.mywallet.Wallethash);

                if (this.exchangedatalist != null && this.exchangedatalist.Count > 0)
                {
                    this.NoData.Visibility = Visibility.Hidden;
                    this.ListData.Visibility = Visibility.Visible;

                    // Processed data
                    foreach (ExchangeResponse data in this.exchangedatalist)
                    {
                        ExchangeData edata = new ExchangeData();
                        edata.excid = data.id;
                        edata.deposit_address_amount_symbol = $"{data.deposit_address} {data.deposit_address} {data.deposit_symbol}";
                        edata.xels_address_amount = $"{data.xels_address} {data.xels_amount} XELS";

                        if (data.status == 0)
                        {
                            edata.showstatus = "0";
                        }
                        else if (data.status != 0)
                        {
                            if(data.status == 0)
                            {
                                edata.status[data.status] = "Wating for deposit";
                                edata.showstatus = edata.status[data.status];
                            }
                            else if (data.status == 1)
                            {
                                edata.status[data.status] = "Pending Exchange";
                                edata.showstatus = edata.status[data.status];
                            }
                            else if (data.status == 2)
                            {
                                edata.status[data.status] = "Complete";
                                edata.showstatus = edata.status[data.status];
                            }
                        }

                        this.exchangelist.Add(edata);
                    }

                    this.ExchangesList.ItemsSource = this.exchangelist; 
                }
                else
                {
                    this.ListData.Visibility = Visibility.Hidden;
                    this.NoData.Visibility = Visibility.Visible;

                    //this.exchangelist = loadtestlist();
                    //this.ExchangesList.ItemsSource = this.Exchangelist;
                }

            }
        }

        //public List<ExchangeData> loadtestlist()
        //{
        //    List<ExchangeData> list = new List<ExchangeData>();
        //    list.Add(new ExchangeData() { excid = "1", deposit_address_amount_symbol = "Abc 2.5 $", xels_address_amount ="Xyz 1 XELS", showstatus = "Pending Exchange" });
        //    list.Add(new ExchangeData() { excid = "2", deposit_address_amount_symbol = "Abc 3 $", xels_address_amount ="Xyz 2 XELS", showstatus = "Complete" });
        //    list.Add(new ExchangeData() { excid = "3", deposit_address_amount_symbol = "Abc 1 $", xels_address_amount ="Xyz .5 XELS", showstatus = "Pending Exchange" });

        //    return list;
        //}

        public async void LoadCreate()
        {
            string addr = await GetUnusedReceiveAddressesAsync(this.baseURL);
            addr = FreshAddress(addr);
        }


        private string FreshAddress(string adr)
        {
            adr = adr.Trim(new char[] { '"' });
            return adr;
        }

        private async Task<string> GetUnusedReceiveAddressesAsync(string path)
        {
            string getUrl = path + $"/wallet/unusedaddress?WalletName={this.walletInfo.walletName}&AccountName=account 0";
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


        public async Task DepositNowAsync(ExchangeData orderitem)
        {
            ExchangeResponse dipositreturn = await GetOrderAsync(orderitem.excid);
        }



        private void DepositButton_Click(object sender, RoutedEventArgs e)
        {
            ExchangeData item = (ExchangeData)((sender as Button)?.Tag as ListViewItem)?.DataContext;
            //MessageBox.Show("Item detail: " + item.excid);
            _ = DepositNowAsync(item);
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

        private void ExchangeOrderSubmitButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
