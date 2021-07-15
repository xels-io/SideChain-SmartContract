using System;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for CreateConfirmMnemonic.xaml
    /// </summary>
    public partial class CreateConfirmMnemonic : Window
    {

        string baseURL = URLConfiguration.BaseURL;//"http://localhost:37221/api/wallet";

        WalletCreation _walletcreateconfirm = new WalletCreation();

        private bool canPassMnemonic = false;

        string[] words;

        int[] randomidx = new int[3];

        public CreateConfirmMnemonic()
        {
            InitializeComponent();
        }

        public CreateConfirmMnemonic(WalletCreation walletcreation)
        {
            InitializeComponent();
            InitializeWalletCreationModel(walletcreation);
            RandomSelect();
        }

        private void InitializeWalletCreationModel(WalletCreation CreateWallet)
        {
            this._walletcreateconfirm.Name = CreateWallet.Name;
            this._walletcreateconfirm.Passphrase = CreateWallet.Passphrase;
            this._walletcreateconfirm.Password = CreateWallet.Password;
            this._walletcreateconfirm.Mnemonic = CreateWallet.Mnemonic;
        }

        #region field property 
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private string valueone;
        public string Valueone
        {
            get { return this.valueone; }
            set { this.valueone = value; OnPropertyChanged("Valueone"); }
        }

        private string valuetwo;
        public string Valuetwo
        {
            get { return this.valuetwo; }
            set { this.valuetwo = value; OnPropertyChanged("Valuetwo"); }
        }

        private string valuethree;


        public string Valuethree
        {
            get { CreateConfirmMnemonic createConfirmMnemonic = this; return createConfirmMnemonic.valuethree; }
            set { this.valuethree = value; OnPropertyChanged("Valuethree"); }
        }
        #endregion


        private void RandomSelect()
        {
            // Initialize array to check

            string[] rowwords = this._walletcreateconfirm.Mnemonic.Split('\"');
            this._walletcreateconfirm.Mnemonic = rowwords[1];
            this.words = rowwords[1].Split(' ');

            //// Random number select
            for (int i = 0; i < 3; i++)
            {
                var idx = new Random().Next(this.words.Length);

                if (!this.randomidx.Contains(idx))
                {
                    this.randomidx[i] = idx;
                }
            }
            int fInd = this.randomidx[0] + 1;
            int sInd = this.randomidx[1] + 1;
            int tInd = this.randomidx[2] + 1;
            this.valueone = "Word number " + fInd;
            this.valuetwo = "Word number " + sInd;
            this.valuethree = "Word number " + tInd;

            this.wordone.Content = this.valueone;
            this.wordtwo.Content = this.valuetwo;
            this.wordthree.Content = this.valuethree;

        }

        public void CheckMnemonic()
        {
            string firstword = this.words[this.randomidx[0]];
            string secondword = this.words[this.randomidx[1]];
            string thirdword = this.words[this.randomidx[2]];

            // Check for validation
            if (this._walletcreateconfirm.Mnemonic != "" && this.word1.Text == firstword &&
                this.word2.Text == secondword && this.word3.Text == thirdword)
            {
                this.canPassMnemonic = true;
            }
            else
            {
                MessageBox.Show("Secret words do not match!");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Create cr = new Create();
            cr.Show();
            this.Close();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CheckMnemonic();

            if (this.canPassMnemonic == true)
            {
                string postUrl = this.baseURL + "/wallet/create";

                HttpResponseMessage response = await URLConfiguration.Client.PostAsync(postUrl, new StringContent(JsonConvert.SerializeObject(this._walletcreateconfirm), Encoding.UTF8, "application/json"));

                var content = await response.Content.ReadAsStringAsync(); 

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Successfully created wallet with Name: {this._walletcreateconfirm.Name}");

                    MainWindow mw = new MainWindow();
                    mw.Show();
                    this.Close();
                }
                else
                {
                    var errors = JsonConvert.DeserializeObject<ErrorModel>(content);

                    foreach (var error in errors.Errors)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
            }
        }

    }
}
