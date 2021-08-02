﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Models.SmartContractModels;
using XelsDesktopWalletApp.Views;
using XelsDesktopWalletApp.Views.layout;
using XelsDesktopWalletApp.Views.Pages;

namespace XelsDesktopWalletApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string baseURL = URLConfiguration.BaseURL;
        
        private WalletLoadRequest UserWallet;
        private  static int enterCount = 0;
        List<WalletLoadRequest> walletList;
        NodeStatusModel NodeStatusModel;

        public MainWindow()
        {
            this.UserWallet = new WalletLoadRequest();
            this.walletList = new List<WalletLoadRequest>();
            InitializeComponent();

            this.NodeStatusModel = new NodeStatusModel();

            this.DataContext = this;
            _=  GetNodeStatus();
           // LoadWalletList();
        } 

        private async Task LoadWalletList()
        {
            try
            {
                string getUrl = this.baseURL + "/wallet/list-wallets";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.comboWallets.ItemsSource = await WalletNamesforDropdown(content);
                    GlobalPropertyModel.ChainCheckMessage = "";
                    if (URLConfiguration.Chain == "-mainchain")
                    {
                        this.labChainCheck.Content = "Xels Main chain wallet / XLC Wallet";
                        GlobalPropertyModel.ChainCheckMessage = "Xels Main chain wallet / XLC Wallet";
                    } else if(URLConfiguration.Chain == "-sidechain")
                    {
                        this.labChainCheck.Content = "Xels Side chain wallet / Carbon Credit Wallet";
                        GlobalPropertyModel.ChainCheckMessage = "Xels Side chain wallet / Carbon Credit Wallet";
                    }
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Message-{e.Message}");
            }
        }


        private async Task GetNodeStatus()
        {
            try
            {
                string getNodeStatusInterval = this.baseURL + $"/node/status";
                var content = "";

                HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getNodeStatusInterval);

                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    this.NodeStatusModel = JsonConvert.DeserializeObject<NodeStatusModel>(content);
                    GlobalPropertyModel.CoinUnit = this.NodeStatusModel.CoinTicker;
                    if (this.NodeStatusModel.FeaturesData.Count > 0)
                    {
                        if (this.NodeStatusModel.FeaturesData[0].Namespace == "Xels.Bitcoin.Base.BaseFeature" && this.NodeStatusModel.FeaturesData[0].State== "Initialized")
                        {
                            _ = LoadWalletList();
                        }
                        else
                        {
                            MessageBox.Show("loading Failed and Failed to start wallet!.");
                            this.decryptButton.Visibility = Visibility.Collapsed;
                            this.laNodeStatusCheck.Content = "loading Failed and Failed to start wallet!.";
                            this.loginInfoGrid.Visibility = Visibility.Collapsed;
                            this.loginInforactaangle.Visibility = Visibility.Collapsed;
                            this.CreateOrReplaceBlock.Visibility = Visibility.Hidden;
                        }

                    }
                    else
                    {
                        MessageBox.Show("loading Failed And Failed to start wallet!.");
                        this.decryptButton.Visibility = Visibility.Collapsed;
                        this.laNodeStatusCheck.Content = "loading Failed and Failed to start wallet!.";
                        this.loginInforactaangle.Visibility = Visibility.Collapsed;
                        this.loginInfoGrid.Visibility = Visibility.Collapsed;
                        this.CreateOrReplaceBlock.Visibility = Visibility.Hidden;
                    }

                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            catch (Exception r)
            {
                throw;
            }
        }


        private async Task<List<WalletLoadRequest>> WalletNamesforDropdown(string data)
        {
            
            var walletNames = JsonConvert.DeserializeObject<WalletLoadRequest>(data);

            foreach (var d in walletNames.WalletNames)
            {
                WalletLoadRequest wlr = new WalletLoadRequest();
                wlr.Name = d;
                this.walletList.Add(wlr);
            }

            return this.walletList;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            MainWindow parentWindow = (MainWindow)Window.GetWindow(this);
            parentWindow.Visibility = Visibility.Collapsed;
            CreateOrRestore cr = new CreateOrRestore();

            cr.ShowDialog();
            Close();
        }

        private async void DecryptButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            this.decryptButton.IsEnabled = false;
           await LoginFunctionAsync();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async Task<string> LoginFunctionAsync()
        {
            string msg = "";
            try
            {

                this.UserWallet.Name = (string)this.comboWallets.SelectedValue;
                this.UserWallet.Password = this.password.Password.Trim();

                if (this.UserWallet.Name != null && this.UserWallet.Password != "")
                {
                    string postUrl = this.baseURL + "/wallet/load/";
                    var content = "";

                    HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(postUrl, this.UserWallet);
                    content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        await GetNodeStatus();
                        GlobalPropertyModel.WalletName = this.UserWallet.Name;

                        MainLayout mainLayout = new MainLayout(this.UserWallet.Name);
                        mainLayout.Show();
                        this.Close();
                    }
                    else if (content != "" || content != null)
                    {
                        LoginError loginError = new LoginError();
                        loginError = JsonConvert.DeserializeObject<LoginError>(content);
                        MessageBox.Show($"{loginError.errors[0].message}");
                    }
                    else
                    {
                        MessageBox.Show($"Error Code{response.StatusCode} : Message - {response.ReasonPhrase}");
                    }
                }
                else
                {
                    MessageBox.Show($"Enter Valid Information.");
                }
                return msg;
            }
            catch (Exception e)
            {
                msg = e.Message.ToString();
                return msg;
            }



        }
        private async void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter && enterCount == 0)
            {
                enterCount = enterCount + 1;
                this.decryptButton.IsEnabled = false;
                await  LoginFunctionAsync();
            }
        }
    }
}
