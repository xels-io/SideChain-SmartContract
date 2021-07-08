using System.Windows;
using System.Windows.Navigation;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for CreateOrRestore.xaml
    /// </summary>
    public partial class CreateOrRestore : Window
    {
        public CreateOrRestore()
        {
            InitializeComponent();
        }

        private void restoreButton_Click(object sender, RoutedEventArgs e)
        {
            Restore restore = new Restore();
            restore.Show();
            this.Close();
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            Create create = new Create();
            create.Show();
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }
    }
}
