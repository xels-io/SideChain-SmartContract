using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace XelsDesktopWalletApp.Models.CommonModels
{
    public class URLConfiguration
    {
        public static string BaseURLMain = "http://localhost:17103/api";  //main chain 
        // api port 37221

        public static string BaseURLSideChain = "http://localhost:37223/api";  //side chain

        public static string BaseURL;//value assagin from program.cs

        public static HttpClient Client = new HttpClient();

        public static string Chain;
    }
}
