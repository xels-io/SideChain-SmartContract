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

        public CoinType CoinType { get; set; }

        public Money AmountConfirmed { get; set; }
        public Money AmountUnconfirmed { get; set; }
        public Money SpendableAmount { get; set; }
        public IEnumerable<AddressModel> Addresses { get; set; }

    }

    public class WalletBalanceArray
    {
        public WalletBalance[] Balances { get; set; }
    }
}
