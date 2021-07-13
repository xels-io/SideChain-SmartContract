using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using Xels.Bitcoin.Features.Wallet;
using Xels.Bitcoin.Features.Wallet.Models;

namespace XelsDesktopWalletApp.Models
{
    public class WalletBalance
    {
        public string AccountName { get; set; }
        public string AccountHdPath { get; set; }

        public string CoinType { get; set; }

        public decimal AmountConfirmed { get; set; }
        public decimal AmountUnconfirmed { get; set; }
        public decimal SpendableAmount { get; set; }
        //public IEnumerable<AddressModel> Addresses { get; set; }

    }

    public class WalletBalanceArray
    {
        public WalletBalance[] Balances { get; set; }
    }
}
