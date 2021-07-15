using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using NBitcoin;

using XelsDesktopWalletApp.Models.SmartContractModels;

namespace XelsDesktopWalletApp.Models
{
    public class TransactionItemModel
    {

        private string _type;
        private string _timeStamp;
        private double _amount;
        private string _amountWithUnit;

        public string Type
        {
            get
            {
                if (_type == "mined")
                {
                    return _type = "POW REWARD";
                }
                else if(_type == "staked")
                {
                    return _type = "HYBRID REWARD";
                }
                else if(_type == "sent")
                {
                    return _type = "SENT";
                }
                else
                {
                    return _type = "RECEIVED";
                }
            }
            set
            {
                _type = value;

            }
        }
        public string ToAddress { get; set; }
        public double Amount {

            get {
                return (this._amount / 100000000);
            }
            set {
                this._amount = value;
            }
        }
        public string AmountWithUnit { get { return this.Amount.ToString()+" " + GlobalPropertyModel.CoinUnit; }   }
        public string Id { get; set; }

        public string Timestamp 
        { 
            get { return _timeStamp; } 
            set {
                _timeStamp = Utils.UnixTimeToDateTime(Convert.ToInt64(value)).ToString("F");
                    //new DateTime(Convert.ToInt32(value), System.DateTimeKind.Utc ).ToString(); //ConvertToDate(value);
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


        public List<PaymentDetailModel> Payments { get; set; } // new added for detail 
        public long TxOutputTime { get; set; } // new added for detail 
        public int TxOutputIndex { get; set; } // new added for detail 
    }
    public class PaymentDetailModel
    {
        public string DestinationAddress { get; set; }
        public double Amount { get; set; }
        public bool IsChange { get; set; }
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
