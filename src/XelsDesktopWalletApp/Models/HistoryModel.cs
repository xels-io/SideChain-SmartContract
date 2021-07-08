using System;
using System.Collections.Generic;
using System.Text;
using Xels.Bitcoin.Features.Wallet;

namespace XelsDesktopWalletApp.Models
{
    public class HistoryModel
    {
        public string AccountName { get; set; }
        public string AccountHdPath { get; set; }
        public CoinType CoinType { get; set; }
        //public List<TransactionItemModel> transactionsHistory { get; set; }
        public TransactionItemModel[] TransactionsHistory { get; set; }
    }

    public class HistoryModelArray
    {
        public HistoryModel[] History { get; set; }
    }
}
