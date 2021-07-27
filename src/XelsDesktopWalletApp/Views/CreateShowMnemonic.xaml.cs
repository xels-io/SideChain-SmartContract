using System;
using System.Windows;
using System.Windows.Controls;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Views.Pages;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for CreateShowMnemonic.xaml
    /// </summary>
    public partial class CreateShowMnemonic : Page
    {
        private string copyMnemonicText; 
        #region Show Mnemonic Property

        WalletCreation _walletcreate = new WalletCreation();

        //private string _mnemonic;
        //public string _Mnemonic
        //{
        //    get { return _mnemonic; }
        //    set { _mnemonic = value; }
        //}

        #endregion

        public CreateShowMnemonic()
        {
            InitializeComponent();
        }

        
        public CreateShowMnemonic(WalletCreation model)
        {
            InitializeComponent();
            string words = model.Mnemonic.Replace("\"", "");
            string[] mn = words.Split(' ');
            string[] mnv = new string[12];
            string[] mnCopy = new string[12];

            for (int i =0; i< mn.Length; i++)
            {
                mnv[i] = $"{ i + 1 }.{" "}{ mn[i] }{" "}";
                mnCopy[i] = mn[i];
            }

             this.textBoxTextToMnemonic.Text = String.Join(" ", mnv); 
            this.copyMnemonicText = String.Join(" ", mnCopy);

            InitializeWalletCreationModel(model);
        }

        private void InitializeWalletCreationModel(WalletCreation CreateWallet)
        {
            this._walletcreate.Name = CreateWallet.Name;
            this._walletcreate.Passphrase = CreateWallet.Passphrase;
            this._walletcreate.Password = CreateWallet.Password;
            this._walletcreate.Mnemonic = CreateWallet.Mnemonic;
        }

        private void copyClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.copyMnemonicText);
        }

        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                CreateOrRestore parentWindow = (CreateOrRestore)Window.GetWindow(this);
                parentWindow.Content = new CreateConfirmMnemonic(this._walletcreate);
            }
            catch (Exception)
            {
                this.IsEnabled = true;
                throw;
            }

            this.IsEnabled = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOrRestore parentWindow = (CreateOrRestore)Window.GetWindow(this);
            parentWindow.Content = new CreateWalletPage();
          
        }
    }
}
