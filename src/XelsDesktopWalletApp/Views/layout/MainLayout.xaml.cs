using System.Windows;
using System.Windows.Input;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.Pages;
using XelsDesktopWalletApp.Views.SmartContractView;
using XelsDesktopWalletApp.Views.ViewPage;


namespace XelsDesktopWalletApp.Views.layout
{
    /// <summary>
    /// Interaction logic for MainLayout.xaml
    /// </summary>
    public partial class MainLayout : Window
    {
        #region Common
        private string baseURL = URLConfiguration.BaseURL;
        private readonly WalletInfo walletInfo = new WalletInfo();
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
        #endregion

        public MainLayout()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public MainLayout(string walletname)
        {
            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;

            InitializeComponent();
            this.DataContext = this;


            //GetGeneralInfoAsync();
            //LoadLoginAsync();
            //GetHistoryAsync();

            //if (URLConfiguration.Chain != "-sidechain")// (!this.sidechainEnabled)
            //{
            //    _ = GetStakingInfoAsync(this.baseURL);
            //}

            //if (URLConfiguration.Chain == "-sidechain")// (!this.sidechainEnabled)
            //{
            //    this.buttonPowMining.Visibility = Visibility.Hidden;
            //}
            //PopulateTxt();
        }

        private void ButtonFechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GridBarraTitulo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            this.PageContent.Content = new DashboardPage(this.walletName);
            this.btnDashboard.Focus();
        }

        private void btn_SmartContract_Click(object sender, RoutedEventArgs e)
        {
            this.PageContent.Content = null;
            this.PageContent.Content = new SmtAddressSelection(this.walletName);
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            this.PageContent.Content = null;
            this.PageContent.Content = new HistoryPage(this.walletName);
        }

        private void btn_Exchange_Click(object sender, RoutedEventArgs e)
        {
            this.PageContent.Content = null;
            this.PageContent.Content = new ExchangePage(this.walletName);
        }

        private void LogOut_Button(object sender, RoutedEventArgs e)
        {
            LogoutConfirm lc = new LogoutConfirm(this.walletName);
            lc.Show();
            this.Close();
        }

        private void AddressBookButton_Click(object sender, RoutedEventArgs e)
        {

            this.PageContent.Content = new AddressBookPage(this.walletName);
            //AddressBook AddressBook = new AddressBook(this.walletName);
            //AddressBook.Show();
            //this.Close();
        }

        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            this.PageContent.Content = new AdvancedPage(this.walletName);
            //Advanced Advanced = new Advanced(this.walletName);
            //Advanced.Show();
            //this.Close();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            this.PageContent.Content = new DashboardPage(this.walletName);
        }


        //private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        //{
        //    this.ButtonCloseMenu.Visibility = Visibility.Visible;
        //    this.ButtonOpenMenu.Visibility = Visibility.Collapsed;
        //    this.GridMain.Width = 880;
        //}

        //private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        //{
        //    this.ButtonCloseMenu.Visibility = Visibility.Collapsed;
        //    this.ButtonOpenMenu.Visibility = Visibility.Visible;
        //    this.GridMain.Width = 1010;

        //}

        //private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //UserControl usc = null;
        //    //GridMain.Children.Clear();

        //    //switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
        //    //{
        //    //    case "ItemHome":
        //    //        usc = new UserControlHome();
        //    //        GridMain.Children.Add(usc);
        //    //        break;
        //    //    case "ItemCreate":
        //    //        usc = new UserControlCreate();
        //    //        GridMain.Children.Add(usc);
        //    //        break;
        //    //    case "Page1":
        //    //        GridMain.Children.Add(new Page1());
        //    //        break;
        //    //    default:
        //    //        break;
        //    //}

        //    Page usc = null;
        //    this.GridMain.Content = null;

        //    switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
        //    {

        //        case "Page1":
        //            this.GridMain.Content = new Page1();
        //            break;
        //        case "Page2":
        //            this.GridMain.Content = new Page2();
        //            break;
        //        default:
        //            break;
        //    }
        //}

    }
}
