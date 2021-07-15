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
using XelsDesktopWalletApp.Views.Pages.SendPages;
using XelsDesktopWalletApp.Views.ViewPage;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddressBookPage.xaml
    /// </summary>
    public partial class AddressBookPage : Page
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
        private AddressLabelArray addressLabelArray = new AddressLabelArray();

        public AddressBookPage()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public AddressBookPage(string walletname)
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

        public async void LoadAddresses()
        {
            await GetAddressBookAddressesAsync(this.baseURL);

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

        private async Task GetAddressBookAddressesAsync(string path)
        {
            string getUrl = path + "/AddressBook";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
            content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    this.addressLabelArray = JsonConvert.DeserializeObject<AddressLabelArray>(content);

                    foreach (var addr in this.addressLabelArray.Addresses)
                    {
                        AddressLabel addressLabel = new AddressLabel();
                        addressLabel.label = addr.label;
                        addressLabel.address = addr.address;
                        this.addresses.Add(addressLabel);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }

            List<AddressLabel> addresslist = ProcessAddresses(content);

        }

        public List<AddressLabel> ProcessAddresses(string _content)
        {
            JObject json = JObject.Parse(_content);

            AddressLabel addresslist = new AddressLabel();

            return null;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = this.AddressList;
            DataGridRow Row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowAndColumn = (DataGridCell)dataGrid.Columns[1].GetCellContent(Row).Parent;
            string CellValue = ((TextBlock)RowAndColumn.Content).Text;


            NavigationService navService = NavigationService.GetNavigationService(this);
            MainchainPage page2Obj = new MainchainPage(this.walletName, CellValue);
            navService.Navigate(page2Obj);
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

                    if (response.IsSuccessStatusCode)
                    {
                        this.NewAddressPopup.IsOpen = false;
                        MessageBox.Show("Successfully created with label: " + newaddress.label);
                        LoadAddresses();
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
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = this.AddressList;
            DataGridRow Row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowAndColumn = (DataGridCell)dataGrid.Columns[0].GetCellContent(Row).Parent;
            string CellValue = ((TextBlock)RowAndColumn.Content).Text;

            DeleteAddressAsync(CellValue);
        }

        private async Task DeleteAddressAsync(string item)
        {
            try
            {
                string postUrl = this.baseURL + "/AddressBook/address?" + item;
                HttpResponseMessage response = await URLConfiguration.Client.DeleteAsync(postUrl);
                // response = {StatusCode: 500, ReasonPhrase: 'Internal Server Error', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully deleted address with label: " + item);
                    LoadAddresses();
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception e)
            {
                throw;
            }
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
