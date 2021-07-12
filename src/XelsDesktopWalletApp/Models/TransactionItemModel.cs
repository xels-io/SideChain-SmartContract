using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using NBitcoin;

namespace XelsDesktopWalletApp.Models
{
    public class TransactionItemModel
    {

        private string _type;
        private string _timeStamp;

        public string Type
        {

            get
            {
                if (_type == "mined")
                {
                    return _type = "POW REWARD";
                }
                else
                {
                    return _type = "HYBRID REWARD";
                }
            }
            set
            {
                _type = value;

            }
        }
        public string ToAddress { get; set; }
        public decimal Amount { get; set; }
        public string Id { get; set; }

        public string Timestamp 
        { 
            get { return _timeStamp; } 
            set {
                _timeStamp = new DateTime(Convert.ToInt32(value), System.DateTimeKind.Utc ).ToString(); //ConvertToDate(value);
            }
            //
            //DateTime.ParseExact(value,
            //                    "yyyyMMdd",
            //                    CultureInfo.InvariantCulture,
            //                    DateTimeStyles.None).ToString("d"); 
        }

        public int? ConfirmedInBlock { get; set; }
        public int? BlockIndex { get; set; }
        public decimal Fee { get; set; }

        // List<TransactionItemModel> TransactionsHistory { get; set; }

        private string ConvertToDate(string d)
        {
            int date = Convert.ToInt32(d);

            int day = date % 100;
            int month = (date / 100) % 100;
            int year = date / 10000;

            var result = Convert.ToString(new DateTime(year, month, day));
            return result;
        }
    }

    public class TransactionItemModelArray
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
