using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for AddressBook.xaml
    /// </summary>
    public partial class AddressBook : Window
    {

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

        string baseURL = URLConfiguration.BaseURL;
        /*"http://localhost:37221/api"*/

        List<AddressLabel> addresses = new List<AddressLabel>();

        public AddressBook()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public AddressBook(string walletname)
        {
            InitializeComponent();

            this.DataContext = this;

            this.walletName = walletname;
            LoadAddresses();
        }


        public bool isValid()
        {
            if (this.LabelTxt.Text == string.Empty)
            {
                MessageBox.Show("Please enter a label for your address.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.LabelTxt.Focus();
                return false;
            }

            if (this.LabelTxt.Text.Length < 2)
            {
                MessageBox.Show("A label needs to be at least 2 characters long.");
                this.LabelTxt.Focus();
                return false;
            }

            if (this.LabelTxt.Text.Length > 40)
            {
                MessageBox.Show("A label can't be more than 40 characters long.");
                this.LabelTxt.Focus();
                return false;
            }

            if (this.AddressTxt.Text == string.Empty)
            {
                MessageBox.Show("Please add a valid address.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.AddressTxt.Focus();
                return false;
            }

            return true;
        }

        //public void AddAddressManually()
        //{
        //    this.addresses = new List<AddressLabel>() {
        //            new AddressLabel { label = "Towsif", address = "0bcds6f9df9gdbfgidbfrfbgfgdsgfdtowsif" },
        //            new AddressLabel { label = "Shuvo", address = "63grkjbfcghsdggdgdgdgdgfhdfdgfdshuvo" },
        //            new AddressLabel { label = "Mts", address = "a7sdf8s7fdfsgdfghgjfdhrfhffhdgffdhmts" }
        //        };
        //    this.AddressList.ItemsSource = this.addresses;

        //    if (this.addresses.Count > 0)
        //    {
        //        this.NoData.Visibility = Visibility.Hidden;
        //        this.ListData.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        this.ListData.Visibility = Visibility.Hidden;
        //        this.NoData.Visibility = Visibility.Visible;
        //    }
        //}

        public async void LoadAddresses()
        {
            this.addresses = await GetAPIAsync(this.baseURL);

            if (this.addresses.Count > 0)
            {
                this.NoData.Visibility = Visibility.Hidden;
                this.ListData.Visibility = Visibility.Visible;
                this.AddressList.ItemsSource = this.addresses;
            }
            else
            {
                this.ListData.Visibility = Visibility.Hidden;
                this.NoData.Visibility = Visibility.Visible;
            }
        }

        private async Task<List<AddressLabel>> GetAPIAsync(string path)
        {
            try
            {
                string getUrl = path + "/AddressBook";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl); // Error : e = {"No connection could be made because the target machine actively refused it."}

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.addresses = JsonConvert.DeserializeObject<List<AddressLabel>>(content);
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }

                List<AddressLabel> addresslist = ProcessAddresses(content);
                return addresslist;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<AddressLabel> ProcessAddresses(string _content)
        {
            JObject json = JObject.Parse(_content);

            AddressLabel addresslist = new AddressLabel();

            return null;
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Send send = new Send();
            send.Show();
            this.Close();
        }

        private void receiveButton_Click(object sender, RoutedEventArgs e)
        {
            Receive receive = new Receive();
            receive.Show();
            this.Close();
        }
        private void createButton_Click(object sender, RoutedEventArgs e)
        {

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
        }
        private void Hyperlink_NavigateLogout(object sender, RequestNavigateEventArgs e)
        {
            LogoutConfirm lc = new LogoutConfirm(this.walletName);
            lc.Show();
            this.Close();
        }

        private void Hyperlink_NavigateAddressBook(object sender, RequestNavigateEventArgs e)
        {
            AddressBook ex = new AddressBook(this.walletName);
            ex.Show();
            this.Close();
        }

        private void Hyperlink_NavigateAdvanced(object sender, RequestNavigateEventArgs e)
        {
            Advanced adv = new Advanced(this.walletName);
            adv.Show();
            this.Close();
        }


        private void AddAddress_Click(object sender, RoutedEventArgs e)
        {
            this.NewAddressPopup.IsOpen = true;
        }

        private void HidePopup_Click(object sender, RoutedEventArgs e)
        {
            this.NewAddressPopup.IsOpen = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NewAddressPopup.IsOpen = false;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            AddressLabel address = new AddressLabel();
            address.label = LabelTxt.Text;
            address.address = AddressTxt.Text;
            AddNewAddress(address);
        }

        public async Task AddNewAddress(AddressLabel newaddress)
        {
            try
            {
                if (isValid())
                {
                    string postUrl = this.baseURL + "/AddressBook/address";

                    HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(newaddress), Encoding.UTF8, "application/json"));
                    // Error : e = {"No connection could be made because the target machine actively refused it."}
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Successfully created with label: " + newaddress.label);

                        this.NewAddressPopup.IsOpen = false;
                    }
                    else
                    {
                        MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Send td = new Send(this.walletName);
            td.Show();
            this.Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            AddressLabel item = (AddressLabel)((sender as Button)?.Tag as ListViewItem)?.DataContext;

            //try
            //{
            //    string postUrl = this.baseURL + "/AddressBook/address";
            //    string deleteParameter = item.label;
            //    HttpResponseMessage response = URLConfiguration.Client.DeleteAsync(postUrl);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        MessageBox.Show("Successfully deleted address with label: " + item.label);
            //    }
            //    else
            //    {
            //        MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw;
            //}
        }


        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = AddressList;
            DataGridRow Row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowAndColumn = (DataGridCell)dataGrid.Columns[1].GetCellContent(Row).Parent;
            string CellValue = ((TextBlock)RowAndColumn.Content).Text;

            Clipboard.SetText(CellValue);
        }
    }

}
