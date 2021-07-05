using System.Windows;

namespace XelsDesktopWalletApp.Views.layout
{
    /// <summary>
    /// Interaction logic for MainLayout.xaml
    /// </summary>
    public partial class MainLayout : Window
    {
        public MainLayout()
        {
            InitializeComponent();
        }

       

        private void MenuItem_ClickPage1(object sender, RoutedEventArgs e)
        {
            this.page_Content.Children.Add(new Page1());
        }

        private void MenuItem_ClickPage2(object sender, RoutedEventArgs e)
        {
            this.page_Content.Children.Add(new Page2());
        }

      
    }
}
