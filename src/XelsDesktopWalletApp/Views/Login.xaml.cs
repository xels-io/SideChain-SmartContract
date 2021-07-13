﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
//using System.Web.Script.Serialization;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
         
        string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api/wallet";
        List<string> listData;

        public List<WalletLoadRequest> _myList { get; set; }
        private List<WalletLoadRequest> myList = new List<WalletLoadRequest>();
        private WalletLoadRequest selectedWallet = new WalletLoadRequest();

        public List<WalletLoadRequest> MyList
        {
            get
            {
                return this.myList;
            }
            set
            {
                this.myList = value;
            }
        }
        public WalletLoadRequest SelectedWallet
        {
            get
            {
                return this.selectedWallet;
            }
            set
            {
                this.selectedWallet = value;
            }
        }

        public Login()
        {
            InitializeComponent();

            this.DataContext = this;

            LoadLogin();
        }

        public async void LoadLogin()
        {
            await GetAPIAsync(this.baseURL);
        }

        private async Task GetAPIAsync(string path)
        {
            string getUrl = path + "/wallet/list-wallets";
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
            converted(content);
        }

        private void converted(string data)
        {
            string[] rowData = data.Split(':');
            string[] rowDataMain = rowData[1].Split('\"');
            this.listData = rowDataMain.ToList();

            foreach (var d in rowDataMain)
            {
                WalletLoadRequest wlr = new WalletLoadRequest();
                wlr.Name = d;
                if (!( d.Contains("[") || d.Contains(",") || d.Contains("]") ))
                {
                    this.myList.Add(wlr);

                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            CreateOrRestore cr = new CreateOrRestore();
           // cr.Show();
            this.Close();
        }
        
        private async void decryptButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.SelectedWallet.Name != null)
            {
                this.selectedWallet.Password = this.password.Password;

                string postUrl = this.baseURL + "/wallet/load/";

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this.SelectedWallet), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully logged in by " + this.SelectedWallet.Name);

                    Dashboard db = new Dashboard();
                    db.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
        }
    }
}
