using System.Windows;
using XelsDesktopWalletApp.Views.layout;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for LogoutConfirm.xaml
    /// </summary>
    public partial class LogoutConfirm : Window
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
        public LogoutConfirm()
        {
            InitializeComponent();
        }
        public LogoutConfirm(string walletname)
        {
            InitializeComponent();
            this.walletName = walletname;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            //Dashboard ds = new Dashboard(this.walletName);
            MainLayout ds = new MainLayout(this.walletName);
            ds.Show();
            this.Close();
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }
    }
}
