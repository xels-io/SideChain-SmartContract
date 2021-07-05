using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace XelsDesktopWalletApp.Models
{
    public class ExchangeResponse
    {
        public string id { get; set; }
        public string xels_address { get; set; }
        public string deposit_address { get; set; }
        public string deposit_symbol { get; set; }
        public Money xels_amount { get; set; }
        public Money deposit_amount { get; set; }
        public string transaction_id { get; set; }
        public int status { get; set; }
    }

    // customized
    public class ExchangeData
    {
        //string[] s = new string[3];
        public string excid { get; set; }
        public string deposit_address_amount_symbol { get; set; }
        public string xels_address_amount { get; set; }
        //public string[] status { get { return this.s; } set { this.s = value; } }
        public string[] status { get; set; }
        public string showstatus { get; set; }
    }


}