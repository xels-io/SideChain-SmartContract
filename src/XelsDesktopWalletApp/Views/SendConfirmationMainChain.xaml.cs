using System.Windows;

using NBitcoin;

using XelsDesktopWalletApp.Models;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for SendConfirmationMainChain.xaml
    /// </summary>
    public partial class SendConfirmationMainChain : Window
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
            decimal amountSent = data.Transaction.FeeAmount - data.TransactionFee;
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
            Send send = new Send(this.walletName);
            send.Show();
            this.Close();
        }


    }
}
