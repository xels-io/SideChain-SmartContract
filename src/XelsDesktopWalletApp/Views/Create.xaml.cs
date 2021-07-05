using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for Create.xaml
    /// </summary>
    public partial class Create : Window
    {

        string baseURL = URLConfiguration.BaseURL; //"http://localhost:37221/api/wallet";
        string _mnemonic;
        bool canProceedPass = false;

        public Create()
        {
            InitializeComponent();
            LoadCreate();
        }

        public async void LoadCreate()
        {
            this._mnemonic = await GetAPIAsync(this.baseURL);
        }
        
        public bool isValid()
        {
            if (this.name.Text == string.Empty)
            {
                MessageBox.Show("Name is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.name.Focus();
                return false;
            }

            if (this.name.Text.Length < 1 || this.name.Text.Length > 24)
            {
                MessageBox.Show("Name should be 1 to 24 characters long");
                this.name.Focus();
                return false;
            }

            // Name: /^[a-zA-Z0-9]*$/
            if (!Regex.IsMatch(this.name.Text, @"^[a-zA-Z0-9]*$"))
            {
                MessageBox.Show("Please enter a valid wallet name. [a-Z] and [0-9] are the only characters allowed.");
                this.name.Focus();
                return false;
            }

            if (this.password.Password == "")
            {
                MessageBox.Show("Password field is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.password.Focus();
                return false;
            }

            if (this.repassword.Password == "")
            {
                MessageBox.Show("Confirm password field is required!", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.repassword.Focus();
                return false;
            }

            // Password:  /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!#$%&'()*+,-./:;<=>?@[\]^_`{|}~])[A-Za-z\d!#$%&'()*+,-./:;<=>?@[\]^_`{|}~]{8,}$/
            if (!Regex.IsMatch(this.password.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!#$%&'()*+,-./:;<=>?@[\]^_`{|}~])[A-Za-z\d!#$%&'()*+,-./:;<=>?@[\]^_`{|}~]{8,}$"))
            {
                MessageBox.Show("A password must contain at least one uppercase letter, one lowercase letter, one number and one special character.");

                this.password.Focus();
                return false;
            }

            if (this.password.Password.Length < 8)
            {
                MessageBox.Show("A password should be at least 8 characters long");
                this.password.Focus();
                return false;
            }

            return true;
        }

        public void CheckPassInput()
        {
            if (this.password.Password == this.repassword.Password)
            {
                this.canProceedPass = true;
            }
            else
            {
                MessageBox.Show("The two passwords must match!");
            }
        }

        private void Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.name.Text.Equals(String.Empty) && this.password.Password.Equals(this.repassword.Password))
                this.createButton.IsEnabled = true;
            else
                this.createButton.IsEnabled = false;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOrRestore cr = new CreateOrRestore();
            cr.Show();
            this.Close();
        }

        private async Task<string> GetAPIAsync(string path)
        {
            string getUrl = path + "/wallet/mnemonic?language=English&wordCount=12";
            var content = "";

            HttpResponseMessage response = await URLConfiguration.Client.GetAsync(getUrl);
            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
            return content;
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid())
            {
                CheckPassInput();

                if (this.canProceedPass == true)
                {

                    WalletCreation creation = new WalletCreation();
                    creation.name = this.name.Text;
                    creation.password = this.password.Password;
                    creation.passphrase = this.passphrase.Text;
                    creation.mnemonic = this._mnemonic;

                    CreateShowMnemonic csm = new CreateShowMnemonic(creation);
                    csm.Show();
                    this.Close();
                }
            }
        }

    }
}
