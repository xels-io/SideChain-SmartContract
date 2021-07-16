using System.Windows;
using System.Windows.Controls;

using NBitcoin;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Views.layout;
using XelsDesktopWalletApp.Views.Pages;

namespace XelsDesktopWalletApp.Views
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
            double amountSent = data.Transaction.FeeAmount - data.TransactionFee;
            this.AmountSent.Content = amountSent;
            this.AmountSentType.Content = data.Cointype;

            this.Fee.Content = data.TransactionFee;
            this.FeeType.Content = data.Cointype;

            this.Total.Content = data.Transaction.FeeAmount;
            this.TotalType.Content = data.Cointype;

            this.Destination.Content = data.Transaction.Recipients[0].DestinationAddress;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainLayout ds = new MainLayout(this.walletName);
            ds.Show();

        }

        private void Rectangle_MouseDown(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

    }
}
