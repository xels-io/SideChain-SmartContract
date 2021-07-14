using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Contracts.Services;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Xels.Bitcoin.Features.Interop.ETHClient;
using XelsDesktopWalletApp.Models;
using Xunit;

namespace XelsDesktopWalletApp.Common
{

    public class  TransactionWallet
    {
        private readonly CreateWallet createWallet = new CreateWallet();

        public async Task<TransactionReceipt> TransferAsync(StoredWallet sWallet, ExchangeResponse exchangeResponse, double amount)
        {
            try
            {
                Wallet wallet = new Wallet();
                var account = new Account(sWallet.PrivateKey);

                //var url = "https://mainnet.infura.io/v3/15851454d7644cff846b1b8701403647";
                var url = "https://kovan.infura.io/v3/15851454d7644cff846b1b8701403647";
                var web3 = new Web3(account, url);

                string contractAddress = "";

                if (sWallet.Coin == "SELS")
                {
                    contractAddress = "0x0E74264EAd02B3a9768Dc4F1A509fA7F49952df6";
                }
                else if (sWallet.Coin == "BELS")
                {
                    contractAddress = "0x6fcf304f636d24ca102ab6e4e4e089115c04ebae";
                }
                else if (sWallet.Coin == "TST")
                {
                    contractAddress = "0xfcb525e2c7351900a204d09bd507a522cebac783";
                }
               string balance = await this.createWallet.GetBalanceAsync(account.Address,sWallet.Coin);

                var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();

                BigInteger amt = (BigInteger)amount * 100000000;
                var transfer = new TransferFunction()
                {
                    FromAddress = sWallet.Address,
                    To = exchangeResponse.deposit_address,
                    TokenAmount = amt,
                };

                var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transfer);
                return transactionReceipt;
            }

            catch (Exception e)
            {
                throw;
            }

        }

    }
}
