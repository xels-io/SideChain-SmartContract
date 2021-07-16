using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Restore.xaml
    /// </summary>
    public partial class Restore : Window
    {

        string baseURL = URLConfiguration.BaseURL;  // "http://localhost:37221/api/wallet";

        public Restore()
        {
            InitializeComponent();
        }       

        public bool isValid()
        {
            if (string.IsNullOrWhiteSpace(this.name.Text))
            {
                MessageBox.Show("Name is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.mnemonic.Text))
            {
                MessageBox.Show("Secret words required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (string.IsNullOrWhiteSpace(this.passphrase.Text))
            {
                MessageBox.Show("Passphrase is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOrRestore cr = new CreateOrRestore();
           // cr.Show();
            this.Close();
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid())
            {
                string postUrl = this.baseURL + "/Wallet/recover";

                WalletRecovery recovery = new WalletRecovery();
                recovery.Name = this.name.Text;
                //recovery.creationDate = creationDate.SelectedDate.Value;
                recovery.CreationDate = this.creationDate.Text;
                recovery.Mnemonic = this.mnemonic.Text;
                recovery.Passphrase = this.passphrase.Text;
                recovery.Password = this.password.Password; 

                HttpResponseMessage response = await URLConfiguration.Client.PostAsJsonAsync(postUrl, recovery);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully saved data with Name:{ recovery.name}");
                }
                else
                {
                    MessageBox.Show($"Error Code{ response.StatusCode } : Message - { response.ReasonPhrase}");
                }
            }
        }
    }
}
