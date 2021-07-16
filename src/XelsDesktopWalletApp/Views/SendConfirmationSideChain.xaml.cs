using System.Windows;
using System.Windows.Controls;

using NBitcoin;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Views.layout;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for SendConfirmationSideChain.xaml
    /// </summary>
    public partial class SendConfirmationSideChain : UserControl
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
            double amountSent = data.Transaction.FeeAmount - data.TransactionFee;
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
            MainLayout ds = new MainLayout(this.walletName);
            ds.Show();

        }

        private void Rectangle_MouseDown(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

    }
}
