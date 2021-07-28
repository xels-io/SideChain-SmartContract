using System.Windows;
using System.Windows.Controls;

using NBitcoin;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.Pages.SendPages;


namespace XelsDesktopWalletApp.Views.Pages.Modals
{
    /// <summary>
    /// Interaction logic for SendUserControl.xaml
    /// </summary>
    public partial class SendUserControl : UserControl
    {
        private readonly string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";

        private readonly WalletInfo walletInfo = new WalletInfo();
        private TransactionSending transactionSending = new TransactionSending();
        private TransactionBuilding transactionBuilding = new TransactionBuilding();

        private WalletBalanceArray balances = new WalletBalanceArray();
        private BuildTransaction buildTransaction = new BuildTransaction();

        private Money totalBalance;
        private Xels.Bitcoin.Features.Wallet.CoinType cointype;
        private Money spendableBalance;

        private Money estimatedFee = 0;
        private bool isSending = false;

        private Money opReturnAmount = 1;

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


        public SendUserControl()
        {
            InitializeComponent();
        }

        public SendUserControl(string walletname)
        {

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            InitializeComponent();
            this.DataContext = this;

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void XELS_Button_Click(object sender, RoutedEventArgs e)
        {

            this.SendFrame.Content = new XelsPage(this.walletName);
        }

        private void SELS_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SendFrame.Content = new SelsPage(this.walletName);
        }

        private void BELS_Button_Click(object sender, RoutedEventArgs e)
        {
           this.SendFrame.Content = new BelsPage(this.walletName);
        }

        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            this.SendFrame.Content = new XelsPage(this.walletName);
        }

        private void Rectangle_MouseDown(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void HidePopup_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
