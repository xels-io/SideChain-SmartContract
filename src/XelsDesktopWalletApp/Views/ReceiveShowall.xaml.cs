using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for ReceiveShowall.xaml
    /// </summary>
    public partial class ReceiveShowall : Window
    {
        #region Base
       
        string baseURL = URLConfiguration.BaseURL;  // "http://localhost:37221/api";
        #endregion

        private readonly WalletInfo walletInfo = new WalletInfo();

        private ReceiveWalletArray addresses = new ReceiveWalletArray();
        private ReceiveWalletArray rcvWalletListsArray = new ReceiveWalletArray();

        private List<string> usedAddressesList = new List<string>();
        private List<string> unusedAddressesList = new List<string>();
        private List<string> changeAddressesList = new List<string>();

        #region Needed property

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

        public ReceiveShowall()
        {
            InitializeComponent();
        }

        public ReceiveShowall(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;

            LoadCreate();

            this.lvDataBinding.ItemsSource = this.unusedAddressesList;
        }

        public async void LoadCreate()
        {
            await GetAPIAsync(this.baseURL);
        }

        private void Hyperlink_NavigateReceive(object sender, RequestNavigateEventArgs e)
        {
            Receive receive = new Receive(this.walletName);
            receive.Show();
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboard = new Dashboard(this.walletName);
            dashboard.Show();
            this.Close();
        }

        private async Task GetAPIAsync(string path)
        {
            string getUrl = path + $"/wallet/addresses?WalletName={this.walletInfo.WalletName}&AccountName=account 0";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);

            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();

                this.addresses = JsonConvert.DeserializeObject<ReceiveWalletArray>(content);
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            ListConvert(this.addresses);

        }

        private void ListConvert(ReceiveWalletArray _content)
        {
            ReceiveWalletArray convertedList = new ReceiveWalletArray();

            this.rcvWalletListsArray.Addresses = _content.Addresses;
            var mylist = this.rcvWalletListsArray.Addresses;


            foreach (ReceiveWalletStatus address in mylist)
            {
                if (address.IsUsed)
                {
                    string usedAddresses = address.Address;
                    this.usedAddressesList.Add(usedAddresses);
                }
                else if (address.IsChange)
                {
                    string changeAddresses = address.Address;
                    this.changeAddressesList.Add(changeAddresses);
                }
                else
                {
                    string unusedAddresses = address.Address;
                    this.unusedAddressesList.Add(unusedAddresses);
                }
            }
        }

        private void btn_copy_Click(object sender, RoutedEventArgs e)
        {
            string item = ((sender as Button)?.Tag as ListViewItem)?.DataContext.ToString();
            Clipboard.SetText(item);

        }

        private void XELS_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SELS_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BELS_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnusedAddr_Button_Click(object sender, RoutedEventArgs e)
        {
            this.lvDataBinding.ItemsSource = this.unusedAddressesList;
        }

        private void ChangeAddr_Button_Click(object sender, RoutedEventArgs e)
        {
            this.lvDataBinding.ItemsSource = this.changeAddressesList;
        }

        private void UsedAddr_Button_Click(object sender, RoutedEventArgs e)
        {
            this.lvDataBinding.ItemsSource = this.usedAddressesList;
        }

    }
}
