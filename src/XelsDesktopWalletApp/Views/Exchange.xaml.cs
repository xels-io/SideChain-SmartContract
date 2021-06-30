using System;
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
        //static HttpClient client = new HttpClient();
        string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";
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
        }


        public void GetOrders(string hash)
        {
            try
            {
                string path = URLConfiguration.BaseURLExchange + "/api/getOrders";

                WebRequest requestObjPost = WebRequest.Create(path);
                requestObjPost.Method = "POST";
                requestObjPost.ContentType = "application/json";

                string postdata = "{"+ hash + "}";

                using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
                {
                    streamWriter.WriteLine(postdata);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)requestObjPost.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                    }

                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public void GetOrder(string orderId)
        {
            try
            {
                string path = URLConfiguration.BaseURLExchange + "/api/getOrder/" + orderId;

                WebRequest requestObjGet = WebRequest.Create(path);
                requestObjGet.Method = "GET";

                HttpWebResponse responseObjGet = null;
                responseObjGet = (HttpWebResponse)requestObjGet.GetResponse();

                string stringresult = null;
                using (Stream stream = responseObjGet.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    stringresult = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public void NewOrder(string data)
        {
            try
            {
                string path = URLConfiguration.BaseURLExchange + "/api/new-order";

                WebRequest requestObjPost = WebRequest.Create(path);
                requestObjPost.Method = "POST";
                requestObjPost.ContentType = "application/json";

                string postdata = "{" + data + "}";

                using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
                {
                    streamWriter.WriteLine(postdata);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)requestObjPost.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                    }

                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void UpdateExchangeList()
        {
            if (this.mywallet.Wallethash != null || this.mywallet.Wallethash != "")
            {
                GetOrders(this.mywallet.Wallethash);
            }
        }

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
