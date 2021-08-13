﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.layout;
using System.Text.RegularExpressions;
using XelsDesktopWalletApp.Common;
using Nethereum.RPC.Eth.DTOs;
using System;
using XelsDesktopWalletApp.Models.SmartContractModels;

namespace XelsDesktopWalletApp.Views.Pages.SendPages
{
    /// <summary>
    /// Interaction logic for BelsPage.xaml
    /// </summary>
    public partial class BelsPage : Page
    {
        private readonly string baseURL = URLConfiguration.BaseURL; // "http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();
        private TransactionSending transactionSending = new TransactionSending();
        private CreateWallet createWallet = new CreateWallet();
        private TransactionWallet transactionWallet = new TransactionWallet();

        public BelsPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public bool isValid()
        {
            if (this.textToAddress.Text.ToString().Trim() == "")
            {
                MessageBox.Show("Address To is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.textToAddress.Focus();
                return false;
            }
            string amt = this.textAmount.Text.Trim();
            if (amt != "")
            {
                var rex = Regex.IsMatch(amt, "[^0-9]+");
                if (rex)
                {
                    MessageBox.Show("Data is not valid");
                    this.textAmount.Focus();
                    return false;
                }
            }
            if (this.textAmount.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Amount is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.textAmount.Focus();
                return false;
            }

            return true;
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendTransactionAsync();
        }

        private async Task SendTransactionAsync()
        {

            try
            {
                if (isValid())
                {
                    var sendResult = new Tuple<TransactionReceipt, string>(null, null);
                    string coinType = this.belsHidden.Text.ToString();
                    StoredWallet localWallateData = this.createWallet.GetLocalWalletDetailsByWalletAndCoin(GlobalPropertyModel.WalletName.ToString(), coinType);
                    if (localWallateData.Address != null)
                    {
                        StoredWallet mWallet = new StoredWallet();

                        string toAddress = this.textToAddress.Text.ToString().Trim();
                        double amount = Convert.ToDouble(this.textAmount.Text);

                        mWallet.Address = localWallateData.Address;
                        mWallet.Walletname = localWallateData.Walletname;
                        mWallet.Coin = localWallateData.Coin;
                        mWallet.Wallethash = localWallateData.Wallethash;
                        mWallet.PrivateKey = Encryption.DecryptPrivateKey(localWallateData.PrivateKey);
                        sendResult = await this.transactionWallet.TransferAsync(mWallet, toAddress, amount);
                        if (sendResult.Item2 == "SUCCESS")
                        {
                            string tranID = sendResult.Item1.TransactionHash.ToString();
                            string message = this.textAmount.Text + " Token successfully send to " + toAddress + "and Transaction Id: " + tranID;
                            MessageBox.Show(message, "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(sendResult.Item2, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("You have not imported yet!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception e)
            {
                GlobalExceptionHandler.SendErrorToText(e);
            }
        }
      
    }
}
