using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Nethereum.Contracts;
using Nethereum.Contracts.Services;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Xels.Bitcoin.Features.Interop.ETHClient;
using XelsDesktopWalletApp.Models;
using XelsDesktopWalletApp.Models.CommonModels;
using Xunit;

namespace XelsDesktopWalletApp.Common
{

    public class  TransactionWallet
    {
        private readonly CreateWallet createWallet = new CreateWallet();

        public async Task<Tuple<TransactionReceipt,string>> TransferAsync(StoredWallet sWallet, ExchangeResponse exchangeResponse, double amount)
        {
            string retMesage = "";
            try
            {
                Wallet wallet = new Wallet();
                var account = new Account(sWallet.PrivateKey);
                double balance = await this.createWallet.GetBalanceMainAsync(account.Address, sWallet.Coin);
                BigInteger bgBalance = (BigInteger)balance;
                if (bgBalance > 0 && bgBalance != 0)
                {
                    var url = URLConfiguration.Wb3URLExchangeKoven;
                    var web3 = new Web3(account, url);

                    string contractAddress = "";

                    if (sWallet.Coin == "SELS")
                    {
                        contractAddress = URLConfiguration.SELSContractAddress;
                    }
                    else if (sWallet.Coin == "BELS")
                    {
                        contractAddress = URLConfiguration.BELSContractAddress;
                    }
                    else if (sWallet.Coin == "TST")
                    {
                        contractAddress = URLConfiguration.TSTContractAddress;
                    }

                    var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();

                    BigInteger amt = (BigInteger)amount * 100000000;
                    var transfer = new TransferFunction()
                    {
                        FromAddress = sWallet.Address,
                        To = exchangeResponse.deposit_address,
                        TokenAmount = amt,
                    };

                    var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transfer);
                    retMesage = "SUCCESS";
                    var retVal = new Tuple<TransactionReceipt, string>(transactionReceipt, retMesage);
                    return retVal;
                }
                else
                {
                    retMesage = "You have Insufficient Balance for this address.";
                    var retVal = new Tuple<TransactionReceipt, string>(null, retMesage);
                    return retVal;
                }
            }

            catch (Exception e)
            {
                retMesage = e.Message.ToString();
                var retVal = new Tuple<TransactionReceipt, string>(null, retMesage);
                return retVal;
            }

        }

    }
}
