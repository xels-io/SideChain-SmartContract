using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using XelsPCHDesktopWalletApp.Models;
using XelsPCHDesktopWalletApp.Models.CommonModels;
using XelsPCHDesktopWalletApp.Views.layout;
using System.Text.RegularExpressions;
using XelsPCHDesktopWalletApp.Common;
using Nethereum.RPC.Eth.DTOs;
using System;
using XelsPCHDesktopWalletApp.Models.SmartContractModels;
using XelsPCHDesktopWalletApp.Views.Pages.Modals;
using XelsPCHDesktopWalletApp.Views.Dialogs.DialogsModel;
using MaterialDesignThemes.Wpf;

namespace XelsPCHDesktopWalletApp.Views.Pages.SendPages
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
                //MessageBox.Show("Address To is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                //this.Bels_Send_Page.Children.Add(new DisplayErrorMessageUserControl("Address To is required!"));

                var errorDialogMessage = ErrorDialogMessage.GetInstance();
                errorDialogMessage.Message = "Address To is required!";
                _=DialogHost.Show(errorDialogMessage, "SendUserControl");

                this.textToAddress.Focus();
                return false;
            }
            string amt = this.textAmount.Text.Trim();
            if (amt != "")
            {
                var rex = Regex.IsMatch(amt, "[^0-9]+");
                if (rex)
                {
                    //MessageBox.Show("Data is not valid");
                    //this.Bels_Send_Page.Children.Add(new DisplayErrorMessageUserControl("Data is not valid"));

                    var errorDialogMessage = ErrorDialogMessage.GetInstance();
                    errorDialogMessage.Message = "Data is not valid";
                    _ = DialogHost.Show(errorDialogMessage, "SendUserControl");

                    this.textAmount.Focus();
                    return false;
                }
            }
            if (this.textAmount.Text.Trim() == string.Empty)
            {
                //MessageBox.Show("Amount is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                //this.Bels_Send_Page.Children.Add(new DisplayErrorMessageUserControl("Amount is required!"));

                var errorDialogMessage = ErrorDialogMessage.GetInstance();
                errorDialogMessage.Message = "Amount is required!";
                _ = DialogHost.Show(errorDialogMessage, "SendUserControl");

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
                            
                            //MessageBox.Show(message, "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information);
                            //this.Bels_Send_Page.Children.Add(new DisplayErrorMessageUserControl(message));

                            var dialogMessage = InfoDialogMessage.GetInstance();
                            dialogMessage.Message = this.textAmount.Text + " Token successfully send to " + toAddress + "and Transaction Id: " + tranID;
                            await DialogHost.Show(dialogMessage, "SendUserControl");
                        }
                        else
                        {
                            //MessageBox.Show(sendResult.Item2, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            //this.Bels_Send_Page.Children.Add(new DisplayErrorMessageUserControl(sendResult.Item2));

                            var errorDialogMessage = ErrorDialogMessage.GetInstance();
                            errorDialogMessage.Message = sendResult.Item2;
                            await DialogHost.Show(errorDialogMessage, "SendUserControl");
                        }
                    }
                    else
                    {
                        //MessageBox.Show("You have not imported yet!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        //this.Bels_Send_Page.Children.Add(new DisplayErrorMessageUserControl("You have not imported yet!"));

                        var errorDialogMessage = ErrorDialogMessage.GetInstance();
                        errorDialogMessage.Message = "You have not imported yet!";
                        await DialogHost.Show(errorDialogMessage, "SendUserControl");
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
