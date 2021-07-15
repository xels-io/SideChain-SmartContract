using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using XelsDesktopWalletApp.Views.Pages.ReceivePages;

namespace XelsDesktopWalletApp.Views.Pages.Modals
{
    /// <summary>
    /// Interaction logic for ReceiveUserControl.xaml
    /// </summary>
    public partial class ReceiveUserControl : UserControl
    {
     
        string baseURL = URLConfiguration.BaseURL; //  "http://localhost:37221/api";

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

        public ReceiveUserControl()
        {
            InitializeComponent();

        }

        public ReceiveUserControl(string walletname)
        {
            this.walletName = walletname;
            this.walletInfo.WalletName = this.walletName;
            InitializeComponent();
            this.DataContext = this;
        }

        private void restoreButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        private void XelsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveContent.Content = new XelsPage(this.walletName);
            this.XelsButton.Focus();
            //Receive r = new Receive(this.walletName);
            //r.Show();
            //this.Close();
        }

        private void selsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveContent.Content = new SelsPage(this.walletName);
            //ReceiveSelsBels rsb = new ReceiveSelsBels(this.walletName);
            //rsb.Show();
            //this.Close();
        }

        private void Rectangle_MouseDown(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void BelsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveContent.Content = new BelsPage(this.walletName);

            //ReceiveSelsBels rsb = new ReceiveSelsBels(this.walletName);
            //rsb.Show();
            //this.Close();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.ReceiveContent.Content = new XelsPage(this.walletName);
                this.XelsButton.Focus();
        }

        //private void XelsButton_Initialized(object sender, EventArgs e)
        //{
        //    this.ReceiveContent.Content = new XelsPage(this.walletName);
        //    this.XelsButton.Focus();
        //}
    }
}
