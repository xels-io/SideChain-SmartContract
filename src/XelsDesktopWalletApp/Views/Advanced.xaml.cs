using System.Windows;
using System.Windows.Navigation;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Advanced.xaml
    /// </summary>
    public partial class Advanced : Window
    {
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

        public Advanced()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public Advanced(string walletname)
        {
            InitializeComponent();
            this.DataContext = this;

            InitialViewLoad();
            this.walletName = walletname;
        }

        private void InitialViewLoad()
        {
            this.AboutSP.Visibility = Visibility.Visible;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Send send = new Send();
            send.Show();
            this.Close();
        }

        private void receiveButton_Click(object sender, RoutedEventArgs e)
        {
            Receive receive = new Receive();
            receive.Show();
            this.Close();
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            //MyPopup.IsOpen = false;
        }

        private void Hyperlink_NavigateDashboard(object sender, RequestNavigateEventArgs e)
        {
            Dashboard ds = new Dashboard(this.walletName);
            ds.Show();
            this.Close();
        }

        private void Hyperlink_NavigateHistory(object sender, RequestNavigateEventArgs e)
        {
            History hs = new History(this.walletName);
            hs.Show();
            this.Close();
        }

        private void Hyperlink_NavigateExchange(object sender, RequestNavigateEventArgs e)
        {
            Exchange ex = new Exchange(this.walletName);
            ex.Show();
            this.Close();
        }

        private void Hyperlink_NavigateSmartContract(object sender, RequestNavigateEventArgs e)
        {
        }

        private void Hyperlink_NavigateLogout(object sender, RequestNavigateEventArgs e)
        {
            LogoutConfirm lc = new LogoutConfirm(this.walletName);
            lc.Show();
            this.Close();
        }

        private void Hyperlink_NavigateAddressBook(object sender, RequestNavigateEventArgs e)
        {
            AddressBook ex = new AddressBook(this.walletName);
            ex.Show();
            this.Close();
        }

        private void Hyperlink_NavigateAdvanced(object sender, RequestNavigateEventArgs e)
        {
            Advanced adv = new Advanced(this.walletName);
            adv.Show();
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Visible;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }

        private void ExtendedPublicKey_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Hidden;
            this.ExtendedPublicKeySP.Visibility = Visibility.Visible;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }

        private void GenerateAddresses_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Hidden;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Visible;
            this.ResyncSP.Visibility = Visibility.Hidden;
        }

        private void Resync_Click(object sender, RoutedEventArgs e)
        {
            this.AboutSP.Visibility = Visibility.Hidden;
            this.ExtendedPublicKeySP.Visibility = Visibility.Hidden;
            this.GenerateAddressesSP.Visibility = Visibility.Hidden;
            this.ResyncSP.Visibility = Visibility.Visible;
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.ExtPubKeyTxt.Text);
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Rescan_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
