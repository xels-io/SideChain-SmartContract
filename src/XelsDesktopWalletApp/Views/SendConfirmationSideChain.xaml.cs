using System.Windows;

using NBitcoin;

using XelsDesktopWalletApp.Models;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for SendConfirmationSideChain.xaml
    /// </summary>
    public partial class SendConfirmationSideChain : Window
    {
        private string walletName;

        public SendConfirmationSideChain()
        {
            InitializeComponent();
        }

        public SendConfirmationSideChain(SendConfirmationSC sendConf, string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            bindData(sendConf);
        }

        private void bindData(SendConfirmationSC data)
        {
            decimal amountSent = data.Transaction.FeeAmount - data.TransactionFee;
            this.AmountSent.Content = amountSent;
            this.AmountSentType.Content = data.cointype;

            this.Fee.Content = data.TransactionFee;
            this.FeeType.Content = data.cointype;

            this.OPreturn.Content = data.OpReturnAmount;
            this.OPreturnType.Content = data.cointype;

            this.Total.Content = data.Transaction.FeeAmount;
            this.TotalType.Content = data.cointype;

            this.DestinationFederation.Content = data.Transaction.Recipients[0].FederationAddress;
            this.DestinationAddress.Content = data.Transaction.OpReturnData;

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            SendSideChain sendSc = new SendSideChain(this.walletName);
            sendSc.Show();
            this.Close();
        }
    }
}
