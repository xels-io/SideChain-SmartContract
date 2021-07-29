using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for RestoreWalletPage.xaml
    /// </summary>
    public partial class RestoreWalletPage : Page
    {
        public RestoreWalletPage()
        {
            InitializeComponent();
        }

        string baseURL = URLConfiguration.BaseURL;  // "http://localhost:37221/api/wallet";
                

        public bool isValid()
        {
            if (string.IsNullOrWhiteSpace(this.name.Text))
            {
                MessageBox.Show("Name is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.mnemonic.Text))
            {
                MessageBox.Show("Field is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.creationDate.Text))
            {
                MessageBox.Show("Date is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.password.Password))
            {
                MessageBox.Show("Password is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.passphrase.Password))
            {
                MessageBox.Show("Passphrase is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
           CreateOrRestore createOrRestore =  new CreateOrRestore();

            createOrRestore.Visibility = Visibility.Visible;
            Window win = (Window)this.Parent;
            win.Close();
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            if (isValid())
            {
                string postUrl = this.baseURL + "/Wallet/recover";

                WalletRecovery recovery = new WalletRecovery();
                recovery.Name = this.name.Text;
                //recovery.creationDate = creationDate.SelectedDate.Value;
                recovery.CreationDate = this.creationDate.Text;
                recovery.Mnemonic = this.mnemonic.Text;
                recovery.Passphrase = this.passphrase.Password;
                recovery.Password = this.password.Password;

                HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(postUrl, recovery);

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Successfully saved data with Name:{ recovery.Name}");
                    CreateOrRestore parentWindow = (CreateOrRestore)Window.GetWindow(this);
                    parentWindow.Visibility = Visibility.Collapsed;
                    MainWindow mw = new MainWindow();
                    mw.Show();
                }
                else
                {
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach(var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);
                    }
                    
                }
            }
            else
            {
                this.IsEnabled = true;
            }
        }
    }
}
