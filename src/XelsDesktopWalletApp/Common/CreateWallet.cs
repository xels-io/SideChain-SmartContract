using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Nethereum.Contracts.Extensions;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System.Numerics;
using Nethereum.Contracts.ContractHandlers;
using Xels.Bitcoin.Features.Interop.ETHClient;
using Nethereum.RPC.Eth.DTOs;
using XelsDesktopWalletApp.Models;


namespace XelsDesktopWalletApp.Common
{

    public class Wallet
    {
        public string Address { get; set; }
        public string PrivateKey { get; set; }
    }

    public class StoredWallet
    {
        public string Address { get; set; }
        public string PrivateKey { get; set; }
        public string Walletname { get; set; }
        public string Coin { get; set; }
        public string Wallethash { get; set; }
    }

    public class CreateWallet
    {
        Token token = new Token();

        public Wallet WalletCreationFromPk(string pKey)
        {
            Wallet wallet = new Wallet();
            var account = new Account(pKey);

            wallet.PrivateKey = pKey;
            wallet.Address = account.Address;

            return wallet;
        }

        public void StoreLocally(Wallet wallet, string walletname, string symbol, string wallethash)
        {
            if (walletname != null)
            {
                List<StoredWallet> storedWallets = new List<StoredWallet>();

                List<StoredWallet> returnedWallets = new List<StoredWallet>();
                returnedWallets = RetrieveWallets();

                if (returnedWallets != null)
                {
                    storedWallets = returnedWallets;
                }

                StoredWallet storedWallet = new StoredWallet();
                storedWallet.Address = wallet.Address;
                storedWallet.PrivateKey = wallet.PrivateKey;
                storedWallet.Walletname = walletname;
                storedWallet.Coin = symbol;
                storedWallet.Wallethash = wallethash;

                storedWallets.Add(storedWallet);
                string JSONresult = JsonConvert.SerializeObject(storedWallets.ToArray(), Formatting.Indented);

                string walletCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string walletFile = Path.Combine(walletCurrentDirectory, @"..\..\..\File\Wallets.json");
                string path = Path.GetFullPath(walletFile);

                if (File.Exists(path))
                {
                    File.Delete(path);

                    using (var sw = new StreamWriter(path, true))
                    {
                        sw.WriteLine(JSONresult.ToString());
                        sw.Close();
                    }
                }
                else if (!File.Exists(path))
                {
                    using (var sw = new StreamWriter(path, true))
                    {
                        sw.WriteLine(JSONresult.ToString());
                        sw.Close();
                    }
                }

            }
        }




        public List<StoredWallet> RetrieveWallets()
        {
            List<StoredWallet> wallets = new List<StoredWallet>();

            string walletCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walletFile = Path.Combine(walletCurrentDirectory, @"..\..\..\File\Wallets.json");
            string path = Path.GetFullPath(walletFile);

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();

                    if (json != "{ }" || json != "")
                    {
                        wallets = JsonConvert.DeserializeObject<List<StoredWallet>>(json);
                    }

                    return wallets;
                }
            }

            return null;
        }

        public StoredWallet GetLocalWallet(string walletname, string symbol)
        {
            StoredWallet wallet = new StoredWallet();

            string walletCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walletFile = Path.Combine(walletCurrentDirectory, @"..\..\..\File\Wallets.json");
            string path = Path.GetFullPath(walletFile);

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<StoredWallet> wallets = new List<StoredWallet>();

                if (json != "")
                {
                    wallets = JsonConvert.DeserializeObject<List<StoredWallet>>(json);
                    wallet = wallets.Where(c => c.Walletname == walletname && c.Coin == symbol).FirstOrDefault();
                }

                return wallet;
            }
        }

        //public void Initialize(string cointype)
        //{
        //    using (StreamReader r = new StreamReader(@"D:\All_Projects\xels-fullnode-wpf-v4\src\XelsDesktopWalletApp\Config\Token.json"))
        //    {
        //        string json = r.ReadToEnd();
        //        List<Token> tokens = new List<Token>();

        //        if (json != null)
        //        {
        //            tokens = JsonConvert.DeserializeObject<List<Token>>(json);
        //            this.token = tokens.Where(c => c.name == cointype).FirstOrDefault();
        //        }

        //    }
        //}


        public async Task<string> GetBalanceAsync(string address, string cointype)
        {
            try
            {
                Web3 web3 = new Web3("https://mainnet.infura.io/v3/15851454d7644cff846b1b8701403647");
                //Web3 web3 = new Web3("https://kovan.infura.io/v3/15851454d7644cff846b1b8701403647"); //test er jonno

                string contractAddress = "";

                if (cointype == "SELS")
                {
                    contractAddress = "0x0E74264EAd02B3a9768Dc4F1A509fA7F49952df6";
                }
                else if (cointype == "BELS")
                {
                    contractAddress = "0x6fcf304f636d24ca102ab6e4e4e089115c04ebae";
                }
                else if (cointype == "TST")//test er jonno
                {
                    contractAddress = "0xfcb525e2c7351900a204d09bd507a522cebac783";
                }

                var balanceOfFunctionMessage = new BalanceOfFunction()
                {
                    Owner = address 
                };

                IContractQueryHandler<BalanceOfFunction> balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                BigInteger balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage);

                double balanceconvert = (double)balance * 0.00000001;
                string balancemain = balanceconvert.ToString();

                return balancemain;
            }

            catch (Exception e)
            {
                throw;
            }
        }

        public StoredWallet GetLocalWalletDetails(string walletname)
        {
            StoredWallet wallet = new StoredWallet();

            string walletCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string walletFile = Path.Combine(walletCurrentDirectory, @"..\..\..\File\Wallets.json");
            string path = Path.GetFullPath(walletFile);

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<StoredWallet> wallets = new List<StoredWallet>();

                if (json != "")
                {
                    wallets = JsonConvert.DeserializeObject<List<StoredWallet>>(json);
                    wallet = wallets.Where(c => c.Walletname == walletname).FirstOrDefault();
                }

                return wallet;
            }
        }


    }


}
