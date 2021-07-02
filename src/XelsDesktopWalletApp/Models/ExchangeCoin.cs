using System;
using System.Collections.Generic;
using System.Text;

namespace XelsDesktopWalletApp.Models
{
    public class ExchangeCoin
    {
        public string Name { get; set; }
    }

    public class PostHash
    {
        public string user_code { get; set; }
    }


    //error
    public class ExchangeErr : Exception
    {
        public error error { get; set; }
        public detail detail { get; set; }
    }
    public class error
    {
        public string err_code { get; set; }
    }

    public class detail
    {
        public user_code user_code { get; set; }
    }
    public class user_code
    {
        public string code { get; set; }
        public string title { get; set; }
        public string message { get; set; }
    }
}
