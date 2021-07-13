using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace XelsDesktopWalletApp.Models
{
    public class BuildTransaction
    {
        public decimal Fee { get; set; }
        public string Hex { get; set; }
        public uint256 TransactionId { get; set; }
    }
}
