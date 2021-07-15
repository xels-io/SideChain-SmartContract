using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using NBitcoin;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.Pages.SendPages;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Send.xaml
    /// </summary>
    public partial class Send : Window
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


        public Send()
        {
            InitializeComponent();
        }

        public Send(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;            
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard ds = new Dashboard(this.walletName);
            ds.Show();
            this.Close();
        }

        private void XELS_Button_Click(object sender, RoutedEventArgs e)
        {

            this.SendFrame.Content = new XelsPage(this.walletName);

            //Send send = new Send(this.walletName);
            //send.Show();
            //this.Close();
        }

        private void SELS_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SendFrame.Content = new SelsPage(this.walletName);
            //SendSelsBels sendsb = new SendSelsBels(this.walletName);
            //sendsb.Show();
            //this.Close();
        }

        private void BELS_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SendFrame.Content = new BelsPage(this.walletName);

            //SendSelsBels sendsb = new SendSelsBels(this.walletName);
            //sendsb.Show();
            //this.Close();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            this.SendFrame.Content = new XelsPage(this.walletName);
        }
    }
}
