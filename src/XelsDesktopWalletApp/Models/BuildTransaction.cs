﻿using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace XelsDesktopWalletApp.Models
{
    public class BuildTransaction
    {
        public double Fee { get; set; }
        public string Hex { get; set; }
        public string TransactionId { get; set; }
    }
}
