using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NBitcoin;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;

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
        public List<StoredWallet> storedWallets = new List<StoredWallet>();
        Token token = new Token();


        //public Wallet WalletCreation(string mnemonic)
        //{
        //    Wallet wallet = new Wallet();

        //    //// creates new mnemonic only
        //    //Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
        //    //ExtKey hdRoot = mnemo.DeriveExtKey("my password");
        //    //Console.WriteLine(mnemo);

        //    //try3
        //    var privateKey = "0000000000000000000000000000000000000000000000000000000000000001";
        //    var account = new Account(privateKey);

        //    wallet.PrivateKey = privateKey;
        //    wallet.Address = account.Address;

        //    return wallet;
        //}


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
                this.storedWallets = RetrieveWallets();

                StoredWallet storedWallet = new StoredWallet();
                storedWallet.Address = wallet.Address;
                storedWallet.PrivateKey = wallet.PrivateKey;
                storedWallet.Walletname = walletname;
                storedWallet.Coin = symbol;
                storedWallet.Wallethash = wallethash;

                this.storedWallets.Add(storedWallet);
                string JSONresult = JsonConvert.SerializeObject(this.storedWallets.ToArray(), Formatting.Indented);

                string path = @"D:\All_Projects\xels-fullnode-wpf-v4\src\XelsDesktopWalletApp\File\Wallets.json";


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
            using (StreamReader r = new StreamReader(@"D:\All_Projects\xels-fullnode-wpf-v4\src\XelsDesktopWalletApp\File\Wallets.json"))
            {
                string json = r.ReadToEnd();
                List<StoredWallet> wallets = new List<StoredWallet>();

                if (json != "{ }" || json != "")
                {
                    wallets = JsonConvert.DeserializeObject<List<StoredWallet>>(json);
                }

                return wallets;
            }
        }

        public StoredWallet GetLocalWallet(string walletname, string symbol)
        {
            StoredWallet wallet = new StoredWallet();

            using (StreamReader r = new StreamReader(@"D:\All_Projects\xels-fullnode-wpf-v4\src\XelsDesktopWalletApp\File\Wallets.json"))
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

        public void Initialize(string tokenname)
        {
            using (StreamReader r = new StreamReader(@"D:\All_Projects\xels-fullnode-wpf-v4\src\XelsDesktopWalletApp\Config\Token.json"))
            {
                string json = r.ReadToEnd();
                List<Token> tokens = new List<Token>();

                if (json != null)
                {
                    tokens = JsonConvert.DeserializeObject<List<Token>>(json);
                    this.token = tokens.Where(c => c.name == tokenname).FirstOrDefault();
                }

            }
        }




        #region web3
        public class StandardTokenDeployment : ContractDeploymentMessage
        {

            public static string BYTECODE = "0x60606040526040516020806106f5833981016040528080519060200190919050505b80600160005060003373ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005081905550806000600050819055505b506106868061006f6000396000f360606040523615610074576000357c010000000000000000000000000000000000000000000000000000000090048063095ea7b31461008157806318160ddd146100b657806323b872dd146100d957806370a0823114610117578063a9059cbb14610143578063dd62ed3e1461017857610074565b61007f5b610002565b565b005b6100a060048080359060200190919080359060200190919050506101ad565b6040518082815260200191505060405180910390f35b6100c36004805050610674565b6040518082815260200191505060405180910390f35b6101016004808035906020019091908035906020019091908035906020019091905050610281565b6040518082815260200191505060405180910390f35b61012d600480803590602001909190505061048d565b6040518082815260200191505060405180910390f35b61016260048080359060200190919080359060200190919050506104cb565b6040518082815260200191505060405180910390f35b610197600480803590602001909190803590602001909190505061060b565b6040518082815260200191505060405180910390f35b600081600260005060003373ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005060008573ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600050819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925846040518082815260200191505060405180910390a36001905061027b565b92915050565b600081600160005060008673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600050541015801561031b575081600260005060008673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005060003373ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000505410155b80156103275750600082115b1561047c5781600160005060008573ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828282505401925050819055508273ffffffffffffffffffffffffffffffffffffffff168473ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a381600160005060008673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008282825054039250508190555081600260005060008673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005060003373ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828282505403925050819055506001905061048656610485565b60009050610486565b5b9392505050565b6000600160005060008373ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000505490506104c6565b919050565b600081600160005060003373ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020600050541015801561050c5750600082115b156105fb5781600160005060003373ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008282825054039250508190555081600160005060008573ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000206000828282505401925050819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a36001905061060556610604565b60009050610605565b5b92915050565b6000600260005060008473ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005060008373ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060005054905061066e565b92915050565b60006000600050549050610683565b9056";

            public StandardTokenDeployment() : base(BYTECODE) { }

            [Parameter("uint256", "totalSupply")]
            public BigInteger TotalSupply { get; set; }
        }

        [Function("balanceOf", "uint256")]
        public class BalanceOfFunction : FunctionMessage
        {
            [Parameter("address", "_owner", 1)]
            public string Owner { get; set; }
        }

        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public BigInteger TokenAmount { get; set; }
        }

        [Event("Transfer")]
        public class TransferEventDTO : IEventDTO
        {
            [Parameter("address", "_from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "_to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 3, false)]
            public BigInteger Value { get; set; }
        }

        #endregion

        public async Task<BigInteger> GetBalanceAsync(StoredWallet wllt)
        {
            // initialize web3
            var url = "http://testchain.nethereum.com:8545";
            var privateKey = wllt.PrivateKey;
            var account = new Account(privateKey);
            var web3 = new Web3(account, url);

            var deploymentMessage = new StandardTokenDeployment
            {
                TotalSupply = 100000
            };

            var deploymentHandler = web3.Eth.GetContractDeploymentHandler<StandardTokenDeployment>();
            var transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);
            var contractAddress = transactionReceipt.ContractAddress;


            var balanceOfFunctionMessage = new BalanceOfFunction()
            {
                Owner = wllt.Address,
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage);

            return balance;
        }




    }


}
