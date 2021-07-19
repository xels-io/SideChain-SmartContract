using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using NBitcoin;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages.SendPages
{
    /// <summary>
    /// Interaction logic for XelsPage.xaml
    /// </summary>
    public partial class XelsPage : Page
    {
        private readonly string baseURL = URLConfiguration.BaseURL;// "http://localhost:37221/api";
        private readonly WalletInfo walletInfo = new WalletInfo();


        private TransactionSending transactionSending = new TransactionSending();
        private TransactionBuilding transactionBuilding = new TransactionBuilding();

        private WalletBalanceArray balances = new WalletBalanceArray();
        private BuildTransaction buildTransaction = new BuildTransaction();

        private decimal totalBalance;
        private string cointype;
        private decimal spendableBalance;

        private decimal estimatedFee = 0;
        private bool isSending = false;

        private decimal opReturnAmount = 1;

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


        public XelsPage()
        {
            InitializeComponent();
        }

        public XelsPage(string walletname)
        {
            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            InitializeComponent();
            this.DataContext = this;           
        }


        private void Mainchain_Button_Click(object sender, RoutedEventArgs e)
        {
            this.xelsPageContent.Content = new MainchainPage(this.walletName);
        }

        private void Sidechain_Button_Click(object sender, RoutedEventArgs e)
        {
            this.xelsPageContent.Content = new SidechainPage(this.walletName);
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            this.xelsPageContent.Content = new MainchainPage(this.walletName);
        }

    }
}
