using System;
using System.Windows;
using System.Windows.Controls;

using NBitcoin;

using XelsPCHDesktopWalletApp.Models;
using XelsPCHDesktopWalletApp.Models.SmartContractModels;
using XelsPCHDesktopWalletApp.Views.layout;
using XelsPCHDesktopWalletApp.Views.Pages;

namespace XelsPCHDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for SendConfirmationMainChain.xaml
    /// </summary>
    public partial class SendConfirmationMainChain : UserControl
    {
        private readonly WalletInfo walletInfo = new WalletInfo();
        private string walletName;
        
        public SendConfirmationMainChain()
        {
            InitializeComponent();
        }

        public SendConfirmationMainChain(SendConfirmation sendConf, string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            bindData(sendConf);
        }

        private void bindData(SendConfirmation data)
        {
            
            this.AmountSent.Content = data.Transaction.Recipients[0].Amount +" " + GlobalPropertyModel.CoinUnit; 
            
            this.Fee.Content = data.TransactionFee / 100000000;
             

            this.Total.Content = (Convert.ToDouble(data.Transaction.FeeAmount) + Convert.ToDouble(data.Transaction.Recipients[0].Amount)).ToString() + " " + GlobalPropertyModel.CoinUnit;
             
            this.Destination.Content = data.Transaction.Recipients[0].DestinationAddress;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Rectangle_MouseDown(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

       
    }
}
