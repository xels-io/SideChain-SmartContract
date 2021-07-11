using System.Windows;

using XelsDesktopWalletApp.Models;

namespace XelsDesktopWalletApp.Views
{
    /// <summary>
    /// Interaction logic for CreateShowMnemonic.xaml
    /// </summary>
    public partial class CreateShowMnemonic : Window
    {

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
            this.textBoxTextToMnemonic.Text = model.Mnemonic;

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
            Clipboard.SetText(this.textBoxTextToMnemonic.Text);
        }

        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            CreateConfirmMnemonic ccm = new CreateConfirmMnemonic(this._walletcreate);
            ccm.Show();
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Create create = new Create();
            create.Show();
            this.Close();
        }
    }
}
