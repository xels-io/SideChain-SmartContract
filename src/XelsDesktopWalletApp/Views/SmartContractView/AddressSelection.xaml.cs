using System.Windows;
using System.Windows.Controls;

namespace XelsDesktopWalletApp.Views.SmartContractView
{
    /// <summary>
    /// Interaction logic for AddressSelection.xaml
    /// </summary>
    public partial class AddressSelection : Page
    {
        public AddressSelection()
        {
            InitializeComponent();
        }
        private void useAddressBtn_Click(object sender, RoutedEventArgs e)
        {
            var smtdash = new SmartContractDashboard();
            this.Content = smtdash;
        }
        //private void dashboardBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    Dashboard mw = new Dashboard();
        //    this.Hide();
        //    mw.ShowDialog();
        //    this.Close();
        //}
    }
}
