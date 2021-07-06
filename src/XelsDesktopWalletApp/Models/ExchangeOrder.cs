using System;
using System.Collections.Generic;
using System.Text;

namespace XelsDesktopWalletApp.Models
{
    public class ExchangeOrder
    {
        public string xels_address { get; set; }
        public string deposit_amount { get; set; }
        public string deposit_symbol { get; set; }
        public string user_code { get; set; }
    }
}
