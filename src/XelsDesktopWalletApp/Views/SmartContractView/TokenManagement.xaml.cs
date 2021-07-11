﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Models.SmartContractModels;

namespace XelsDesktopWalletApp.Views.SmartContractView
{
    /// <summary>
    /// Interaction logic for TokenManagement.xaml
    /// </summary>
    public partial class TokenManagement : UserControl
    {
        #region Base
        static HttpClient client = new HttpClient();
        string baseURL = URLConfiguration.BaseURL;// Common Url
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

        public TokenManagement()
        {
            InitializeComponent();
        }

        public TokenManagement(string walletname, string selectedAddress)
        {
            InitializeComponent();
            this.DataContext = this;
            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            this.lab_ActiveAddress.Content = selectedAddress;
            LoadCreateAsync();
        }


        public async void LoadCreateAsync()
        {
            string activeAddress = this.lab_ActiveAddress.Content.ToString();
            await GetAddressBalanceAsync(activeAddress);
            //await GetHistoryAsync(GLOBALS.Address, this.walletName);
           AddTokenList();
        }

        private async Task<string> GetAddressBalanceAsync(string address)
        {
            string getUrl = URLConfiguration.BaseURL + $"/SmartContractWallet/address-balance?address={address}";
            var content = "";
            try
            {
                HttpResponseMessage response = await client.GetAsync(getUrl);
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.lab_addBalance.Content = content;
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                    this.lab_addBalance.Content = "000000000000";
                    GLOBALS.AddressBalance = "000000000000";
                }
            }
            catch (Exception e)
            {

                throw;
            }


            return content;
        }

        private void Btn_AddToken_Click(object sender, RoutedEventArgs e)
        {
            this.tokenManagementPageContant.Children.Add(new AddToken(this.walletName));
        }


        private void Button_CrateTokenClick(object sender, RoutedEventArgs e)
        {
            string activeAddress = this.lab_ActiveAddress.Content.ToString();
            string balance = this.lab_addBalance.Content.ToString();
            this.tokenManagementPageContant.Children.Add(new IssueToken(this.walletName, activeAddress, balance));
        }



        private void Button_CopyClick(object sender, RoutedEventArgs e)
        {
            string activeAddress = this.lab_ActiveAddress.Content.ToString();
            Clipboard.SetText(activeAddress);
            MessageBox.Show(activeAddress + "  COPIED");
        }

        public void AddTokenList()
        {
            string retMsg = "";
            List<TokenRetrieveModel> tokenlist = new List<TokenRetrieveModel>();
            try
            {
                string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string tokenFilePath = System.IO.Path.Combine(CurrentDirectory, @"..\..\..\Token\TokenFile.txt");
                string path = System.IO.Path.GetFullPath(tokenFilePath);

                if (File.Exists(path))
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();

                      
                            string concateData = '[' + json + ']';
                            tokenlist = JsonConvert.DeserializeObject<List<TokenRetrieveModel>>(concateData);
                            this.DataGrid1.ItemsSource = tokenlist;
                        
                        
                    }
                }
            }
            catch (Exception e)
            {
                retMsg = e.Message.ToString();

            }

        }

        private void btn_Delete_Token_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = this.DataGrid1;
            DataGridRow Row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowAndColumn = (DataGridCell)dataGrid.Columns[3].GetCellContent(Row).Parent;
            string CellValue = ((TextBlock)RowAndColumn.Content).Text;
            DeleteToken(CellValue);
            //this.DataGrid1.ItemsSource = null;
            AddTokenList();
        }


        public string DeleteToken(string address)
        {
            string msg = "";
            string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tokenFilePath = System.IO.Path.Combine(CurrentDirectory, @"..\..\..\Token\TokenFile.txt");
            string path = System.IO.Path.GetFullPath(tokenFilePath);

            List<TokenRetrieveModel> tokenlist = new List<TokenRetrieveModel>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    string concateData = '[' + json + ']';
                    tokenlist = JsonConvert.DeserializeObject<List<TokenRetrieveModel>>(concateData);

                    foreach (var item in tokenlist)
                    {
                        if (item.Address == address)
                        {
                            tokenlist.Remove(item);
                            break;
                        }
                    }

                }

                File.Delete(path);
                if (tokenlist.Count > 0)
                {
                    string JSONresult = JsonConvert.SerializeObject(tokenlist, Formatting.Indented);

                    string FinalResult = JSONresult.TrimStart('[').TrimEnd(']').TrimStart().TrimEnd();
                    File.Create(path).Dispose();

                    using (var sw = new StreamWriter(path, true))
                    {
                        sw.WriteLine(FinalResult.ToString());
                        sw.WriteLine(",");
                        sw.Flush();
                        sw.Close();

                    }


                }

            }
            return msg;
        }

    }
}