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
            this.textBoxTextToMnemonic.Text = model.mnemonic;

            InitializeWalletCreationModel(model);
        }

        private void InitializeWalletCreationModel(WalletCreation cr)
        {
            this._walletcreate.name = cr.name;
            this._walletcreate.passphrase = cr.passphrase;
            this._walletcreate.password = cr.password;
            this._walletcreate.mnemonic = cr.mnemonic;
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
