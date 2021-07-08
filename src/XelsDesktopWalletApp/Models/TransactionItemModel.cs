using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace XelsDesktopWalletApp.Models
{
    public class TransactionItemModel
    {
        public TransactionItemType Type { get; set; }
        public string ToAddress { get; set; }
        public Money Amount { get; set; }
        public uint256 Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int? ConfirmedInBlock { get; set; }
        public int? BlockIndex { get; set; }
        public Money Fee { get; set; }
    }

    public  class TransactionItemModelArray  
    {
        public TransactionItemModel[] Transactions { get; set; }
    }

    public enum TransactionItemType
    {
        Received,
        Send,
        Staked,
        Mined
    }
    

}
