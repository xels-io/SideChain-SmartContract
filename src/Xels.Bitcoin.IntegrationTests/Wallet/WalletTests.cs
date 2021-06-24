using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Xels.Bitcoin.Features.Wallet;
using Xels.Bitcoin.Features.Wallet.Controllers;
using Xels.Bitcoin.Features.Wallet.Interfaces;
using Xels.Bitcoin.Features.Wallet.Models;
using Xels.Bitcoin.IntegrationTests.Common;
using Xels.Bitcoin.IntegrationTests.Common.EnvironmentMockUpHelpers;
using Xels.Bitcoin.IntegrationTests.Common.ReadyData;
using Xels.Bitcoin.Networks;
using Xels.Bitcoin.Tests.Common;
using Xels.Bitcoin.Utilities.JsonErrors;
using Xunit;

namespace Xels.Bitcoin.IntegrationTests.Wallet
{
    public class WalletTests
    {
        private const string Password = "password";
        private const string WalletName = "mywallet";
        private const string Passphrase = "passphrase";
        private const string Account = "account 0";
        private readonly Network network;

        public WalletTests()
        {
            this.network = new BitcoinRegTest();
        }

        [Fact]
        public async Task WalletCanReceiveAndSendCorrectlyAsync()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsSender = builder.CreateXelsPowNode(this.network).WithWallet().Start();
                CoreNode XelsReceiver = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                int maturity = (int)XelsSender.FullNode.Network.Consensus.CoinbaseMaturity;
                TestHelper.MineBlocks(XelsSender, maturity + 1 + 5);

                // The mining should add coins to the wallet
                long total = XelsSender.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 6 * 50, total);

                // Sync both nodes
                TestHelper.ConnectAndSync(XelsSender, XelsReceiver);

                // Send coins to the receiver
                HdAddress sendto = XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                Transaction trx = XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network,
                    new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 100, FeeType.Medium, 101));

                // Broadcast to the other node
                await XelsSender.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(trx.ToHex()));

                // Wait for the transaction to arrive
                TestBase.WaitLoop(() => XelsReceiver.CreateRPCClient().GetRawMempool().Length > 0);
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Any());

                long receivetotal = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 100, receivetotal);

                // Check that on the sending node, the Spendable Balance includes unconfirmed transactions.
                // The transaction will have consumed 3 outputs, leaving us 3, and will also return us some as change.
                // Change is always the First output because Shuffle is false!
                Money expectedSenderSpendableBalance = Money.COIN * 3 * 50 + trx.Outputs.First().Value;
                AccountBalance senderBalance = XelsSender.FullNode.WalletManager().GetBalances(WalletName).First();
                Assert.Equal(expectedSenderSpendableBalance, senderBalance.SpendableAmount);

                Assert.Null(XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).First().Transaction.BlockHeight);

                // Generate two new blocks so the transaction is confirmed
                TestHelper.MineBlocks(XelsSender, 2);

                // Wait for block repo for block sync to work
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));

                Assert.Equal(Money.Coins(100), XelsReceiver.FullNode.WalletManager().GetBalances(WalletName, Account).Single().AmountConfirmed);
            }
        }

        [Fact]
        public void WalletBalanceCorrectWhenOnlySomeUnconfirmedAreIncludedInABlock()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsSender = builder.CreateXelsPowNode(this.network).WithWallet().Start();
                CoreNode XelsReceiver = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                int maturity = (int)XelsSender.FullNode.Network.Consensus.CoinbaseMaturity;
                TestHelper.MineBlocks(XelsSender, maturity + 1 + 5);

                // The mining should add coins to the wallet
                long total = XelsSender.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 6 * 50, total);

                // Sync both nodes
                TestHelper.ConnectAndSync(XelsSender, XelsReceiver);

                // Send coins to the receiver
                HdAddress sendto = XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                Transaction trx = XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network,
                    new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 100, FeeType.Medium, 101));

                // Broadcast to the other node
                XelsSender.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(trx.ToHex()));

                // Wait for the transaction to arrive
                TestBase.WaitLoop(() => XelsReceiver.CreateRPCClient().GetRawMempool().Length > 0);
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Any());

                long receivetotal = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 100, receivetotal);

                // Generate two new blocks so the transaction is confirmed
                TestHelper.MineBlocks(XelsSender, 2);
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));


                // Send 1 transaction from the second node and let it get to the first.
                sendto = XelsSender.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                Transaction testTx1 = XelsReceiver.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network,
                    new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 10, FeeType.Medium, 0));
                XelsReceiver.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(testTx1.ToHex()));
                TestBase.WaitLoop(() => XelsSender.CreateRPCClient().GetRawMempool().Length > 0);

                // Disconnect so the first node doesn't get any more transactions.
                TestHelper.Disconnect(XelsReceiver, XelsSender);

                // Send a second unconfirmed transaction on the second node which consumes the first.
                Transaction testTx2 = XelsReceiver.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network,
                    new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 10, FeeType.Medium, 0));
                XelsReceiver.FullNode.NodeService<IBroadcasterManager>().BroadcastTransactionAsync(testTx2);

                // Now we can mine a block on the first node with only 1 of the transactions in it.
                TestHelper.MineBlocks(XelsSender, 1);

                // Connect the nodes again. 
                TestHelper.Connect(XelsSender, XelsReceiver);

                // Second node receives a block with only one transaction in it.
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender, true));

                // Now lets see what is in the second node's wallet!
                IEnumerable<UnspentOutputReference> spendableTxs = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName);

                // There should be one spendable transaction. And it should be testTx2.
                Assert.Single(spendableTxs);
                Assert.Equal(testTx2.GetHash(), spendableTxs.First().Transaction.Id);

                // It follows that if the above assert was violated we would have conflicts when we build a transaction. 
                // Specifically what we don't want is to have testTx1 in our spendable transactions, which was causing the known issue.
            }
        }

        [Fact]
        public void WalletValidatesIncorrectPasswordAfterCorrectIsUsed()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsSender = builder.CreateXelsPowNode(this.network).WithWallet().Start();
                CoreNode XelsReceiver = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                int maturity = (int)XelsSender.FullNode.Network.Consensus.CoinbaseMaturity;
                TestHelper.MineBlocks(XelsSender, maturity + 1 + 5);

                // The mining should add coins to the wallet
                long total = XelsSender.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 6 * 50, total);

                // Sync both nodes
                TestHelper.ConnectAndSync(XelsSender, XelsReceiver);

                // Build a transaction using the correct password.
                HdAddress sendto = XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                Transaction trx = XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network,
                    new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 100, FeeType.Medium, 101));

                // Build a transaction using an incorrect password. It should throw an exception.
                SecurityException exception = Assert.Throws<SecurityException>(() =>
                {
                    Transaction trx2 = XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(
                        XelsSender.FullNode.Network,
                        new WalletAccountReference(WalletName, Account), "Wrong", sendto.ScriptPubKey, Money.COIN * 100,
                        FeeType.Medium, 101));
                });

                Assert.StartsWith("Invalid password", exception.Message);
            }
        }

        [Fact]
        public void WalletCanReorg()
        {
            // This test has 4 parts:
            // Send first transaction from one wallet to another and wait for it to be confirmed
            // Send a second transaction and wait for it to be confirmed
            // Connect to a longer chain that causes a reorg so that the second trasnaction is undone
            // Mine the second transaction back in to the main chain
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsSender = builder.CreateXelsPowNode(this.network).WithWallet().Start();
                CoreNode XelsReceiver = builder.CreateXelsPowNode(this.network).WithWallet().Start();
                CoreNode XelsReorg = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                int maturity = (int)XelsSender.FullNode.Network.Consensus.CoinbaseMaturity;
                TestHelper.MineBlocks(XelsSender, maturity + 1 + 15);

                int currentBestHeight = maturity + 1 + 15;

                // The mining should add coins to the wallet.
                long total = XelsSender.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 16 * 50, total);

                // Sync all nodes.
                TestHelper.ConnectAndSync(XelsReceiver, XelsSender);
                TestHelper.ConnectAndSync(XelsReceiver, XelsReorg);
                TestHelper.ConnectAndSync(XelsSender, XelsReorg);

                // Build Transaction 1.
                // Send coins to the receiver.
                HdAddress sendto = XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                Transaction transaction1 = XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network, new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 100, FeeType.Medium, 101));

                // Broadcast to the other node.
                XelsSender.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(transaction1.ToHex()));

                // Wait for the transaction to arrive.
                TestBase.WaitLoop(() => XelsReceiver.CreateRPCClient().GetRawMempool().Length > 0);
                Assert.NotNull(XelsReceiver.CreateRPCClient().GetRawTransaction(transaction1.GetHash(), null, false));
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Any());

                long receivetotal = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 100, receivetotal);
                Assert.Null(XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).First().Transaction.BlockHeight);

                // Generate two new blocks so the transaction is confirmed.
                TestHelper.MineBlocks(XelsSender, 1);
                int transaction1MinedHeight = currentBestHeight + 1;
                TestHelper.MineBlocks(XelsSender, 1);
                currentBestHeight = currentBestHeight + 2;

                // Wait for block repo for block sync to work.
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsReorg));
                Assert.Equal(currentBestHeight, XelsReceiver.FullNode.ChainIndexer.Tip.Height);
                TestBase.WaitLoop(() => transaction1MinedHeight == XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).First().Transaction.BlockHeight);

                // Build Transaction 2.
                // Remove the reorg node.
                TestHelper.Disconnect(XelsReceiver, XelsReorg);
                TestHelper.Disconnect(XelsSender, XelsReorg);

                ChainedHeader forkblock = XelsReceiver.FullNode.ChainIndexer.Tip;

                // Send more coins to the wallet
                sendto = XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                Transaction transaction2 = XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(CreateContext(XelsSender.FullNode.Network, new WalletAccountReference(WalletName, Account), Password, sendto.ScriptPubKey, Money.COIN * 10, FeeType.Medium, 101));
                XelsSender.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(transaction2.ToHex()));

                // Wait for the transaction to arrive
                TestBase.WaitLoop(() => XelsReceiver.CreateRPCClient().GetRawMempool().Length > 0);
                Assert.NotNull(XelsReceiver.CreateRPCClient().GetRawTransaction(transaction2.GetHash(), null, false));
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Any());
                long newamount = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(Money.COIN * 110, newamount);
                Assert.Contains(XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName), b => b.Transaction.BlockHeight == null);

                // Mine more blocks so it gets included in the chain.
                TestHelper.MineBlocks(XelsSender, 1);
                int transaction2MinedHeight = currentBestHeight + 1;
                TestHelper.MineBlocks(XelsSender, 1);
                currentBestHeight = currentBestHeight + 2;
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                Assert.Equal(currentBestHeight, XelsReceiver.FullNode.ChainIndexer.Tip.Height);
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Any(b => b.Transaction.BlockHeight == transaction2MinedHeight));

                // Create a reorg by mining on two different chains.
                // Advance both chains, one chain is longer.
                TestHelper.MineBlocks(XelsSender, 2);
                TestHelper.MineBlocks(XelsReorg, 10);
                currentBestHeight = forkblock.Height + 10;

                // Connect the reorg chain.
                TestHelper.Connect(XelsReceiver, XelsReorg);
                TestHelper.Connect(XelsSender, XelsReorg);

                // Wait for the chains to catch up.
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsReorg, true));
                Assert.Equal(currentBestHeight, XelsReceiver.FullNode.ChainIndexer.Tip.Height);

                // Ensure wallet reorg completes.
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().WalletTipHash == XelsReorg.CreateRPCClient().GetBestBlockHash());

                // Check the wallet amount was rolled back.
                long newtotal = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(receivetotal, newtotal);
                TestBase.WaitLoop(() => maturity + 1 + 16 == XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).First().Transaction.BlockHeight);

                // ReBuild Transaction 2.
                // After the reorg transaction2 was returned back to mempool.
                XelsSender.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(transaction2.ToHex()));
                TestBase.WaitLoop(() => XelsReceiver.CreateRPCClient().GetRawMempool().Length > 0);

                // Mine the transaction again.
                TestHelper.MineBlocks(XelsSender, 1);
                transaction2MinedHeight = currentBestHeight + 1;
                TestHelper.MineBlocks(XelsSender, 1);
                currentBestHeight = currentBestHeight + 2;

                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsReorg));

                Assert.Equal(currentBestHeight, XelsReceiver.FullNode.ChainIndexer.Tip.Height);
                long newsecondamount = XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(newamount, newsecondamount);
                TestBase.WaitLoop(() => XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Any(b => b.Transaction.BlockHeight == transaction2MinedHeight));
            }
        }

        [Fact]
        public void BuildTransaction_From_ManyUtxos_EnoughFundsForFee()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode node1 = builder.CreateXelsPowNode(this.network).WithWallet().Start();
                CoreNode node2 = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                int maturity = (int)node1.FullNode.Network.Consensus.CoinbaseMaturity;
                TestHelper.MineBlocks(node1, maturity + 1 + 15);

                // The mining should add coins to the wallet.
                long total = node1.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName).Sum(s => s.Transaction.Amount);
                Assert.Equal(16 * (long)this.network.Consensus.ProofOfWorkReward, total);

                // Sync all nodes.
                TestHelper.ConnectAndSync(node1, node2);

                const int utxosToSend = 500;
                const int howManyTimes = 8;

                for (int i = 0; i < howManyTimes; i++)
                {
                    HdAddress sendto = node2.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(WalletName, Account));
                    SendManyUtxosTransaction(node1, sendto.ScriptPubKey, Money.FromUnit(907700, MoneyUnit.Satoshi), utxosToSend);
                }

                TestBase.WaitLoop(() => node1.CreateRPCClient().GetRawMempool().Length == howManyTimes);
                TestHelper.MineBlocks(node1, 1);
                TestHelper.WaitForNodeToSync(node1, node2);

                var transactionsToSpend = node2.FullNode.WalletManager().GetSpendableTransactionsInWallet(WalletName);
                Assert.Equal(utxosToSend * howManyTimes, transactionsToSpend.Count());

                // Firstly, build a tx with value 1. Previously this would fail as the WalletTransactionHandler didn't pass enough UTXOs.
                IActionResult result = node2.FullNode.NodeController<WalletController>().BuildTransaction(
                    new BuildTransactionRequest
                    {
                        WalletName = WalletName,
                        AccountName = "account 0",
                        FeeAmount = "0.1",
                        Password = Password,
                        Recipients = new List<RecipientModel>
                        {
                            new RecipientModel
                            {
                                Amount = "1",
                                DestinationAddress = node1.FullNode.WalletManager()
                                    .GetUnusedAddress(new WalletAccountReference(WalletName, Account)).Address
                            }
                        }
                    }).GetAwaiter().GetResult();

                JsonResult jsonResult = (JsonResult)result;
                Assert.NotNull(((WalletBuildTransactionModel)jsonResult.Value).TransactionId);
            }
        }

        /// <summary>
        /// GivenNodeHadAReorg_And_WalletTipIsBehindConsensusTip_When_ANewBlockArrives_Then_WalletCanRecover
        /// </summary>
        [Fact]
        public void NodeReorgWalletScenario1()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsSender = builder.CreateXelsPowNode(this.network).WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();
                CoreNode XelsReceiver = builder.CreateXelsPowNode(this.network).Start();
                CoreNode XelsReorg = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                // Sync all nodes.
                TestHelper.ConnectAndSync(XelsReceiver, XelsSender);
                TestHelper.ConnectAndSync(XelsReceiver, XelsReorg);
                TestHelper.ConnectAndSync(XelsSender, XelsReorg);

                // Remove the reorg node.
                TestHelper.Disconnect(XelsReceiver, XelsReorg);
                TestHelper.Disconnect(XelsSender, XelsReorg);

                // Create a reorg by mining on two different chains.
                // Advance both chains, one chain is longer.
                TestHelper.MineBlocks(XelsSender, 2);
                TestHelper.MineBlocks(XelsReorg, 10);

                // Connect the reorg chain.
                TestHelper.ConnectAndSync(XelsReceiver, XelsReorg);
                TestHelper.ConnectAndSync(XelsSender, XelsReorg);

                // Wait for the chains to catch up.
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsReorg));
                Assert.Equal(20, XelsReceiver.FullNode.ChainIndexer.Tip.Height);

                TestHelper.MineBlocks(XelsSender, 5);

                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                Assert.Equal(25, XelsReceiver.FullNode.ChainIndexer.Tip.Height);
            }
        }

        /// <summary>
        /// Given_TheNodeHadAReorg_And_ConsensusTipIsdifferentFromWalletTip_When_ANewBlockArrives_Then_WalletCanRecover
        /// </summary>
        [Fact]
        public void NodeReorgWalletScenario2()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsSender = builder.CreateXelsPowNode(this.network).WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();
                CoreNode XelsReceiver = builder.CreateXelsPowNode(this.network).Start();
                CoreNode XelsReorg = builder.CreateXelsPowNode(this.network).WithDummyWallet().Start();

                // Sync all nodes.
                TestHelper.ConnectAndSync(XelsReceiver, XelsSender);
                TestHelper.ConnectAndSync(XelsReceiver, XelsReorg);
                TestHelper.ConnectAndSync(XelsSender, XelsReorg);

                // Remove the reorg node and wait for node to be disconnected.
                TestHelper.Disconnect(XelsReceiver, XelsReorg);
                TestHelper.Disconnect(XelsSender, XelsReorg);

                // Create a reorg by mining on two different chains.
                // Advance both chains, one chain is longer.
                TestHelper.MineBlocks(XelsSender, 2);
                TestHelper.MineBlocks(XelsReorg, 10);

                // Connect the reorg chain.
                TestHelper.ConnectAndSync(XelsReceiver, XelsReorg);
                TestHelper.ConnectAndSync(XelsSender, XelsReorg);

                // Wait for the chains to catch up.
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsReorg));
                Assert.Equal(20, XelsReceiver.FullNode.ChainIndexer.Tip.Height);

                // Rewind the wallet in the XelsReceiver node.
                (XelsReceiver.FullNode.NodeService<IWalletSyncManager>() as WalletSyncManager).SyncFromHeight(10);

                TestHelper.MineBlocks(XelsSender, 5);

                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsReceiver, XelsSender));
                Assert.Equal(25, XelsReceiver.FullNode.ChainIndexer.Tip.Height);
            }
        }

        [Fact]
        public void WalletCanCatchupWithBestChain()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode Xelsminer = builder.CreateXelsPowNode(this.network).WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();

                // Push the wallet back.
                Xelsminer.FullNode.NodeService<IWalletSyncManager>().SyncFromHeight(5);

                TestHelper.MineBlocks(Xelsminer, 5);
            }
        }

        [Fact(Skip = "Investigate PeerConnector shutdown timeout issue")]
        public void WalletCanRecoverOnStartup()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsNodeSync = builder.CreateXelsPowNode(this.network).WithWallet().Start();

                TestHelper.MineBlocks(XelsNodeSync, 10);

                // Set the tip of best chain some blocks in the past
                XelsNodeSync.FullNode.ChainIndexer.SetTip(XelsNodeSync.FullNode.ChainIndexer.GetHeader(XelsNodeSync.FullNode.ChainIndexer.Height - 5));

                // Stop the node (it will persist the chain with the reset tip)
                XelsNodeSync.FullNode.Dispose();

                CoreNode newNodeInstance = builder.CloneXelsNode(XelsNodeSync);

                // Load the node, this should hit the block store recover code
                newNodeInstance.Start();

                // Check that store recovered to be the same as the best chain.
                Assert.Equal(newNodeInstance.FullNode.ChainIndexer.Tip.HashBlock, newNodeInstance.FullNode.WalletManager().WalletTipHash);
            }
        }

        public static TransactionBuildContext CreateContext(Network network, WalletAccountReference accountReference, string password,
            Script destinationScript, Money amount, FeeType feeType, int minConfirmations)
        {
            return new TransactionBuildContext(network)
            {
                AccountReference = accountReference,
                MinConfirmations = minConfirmations,
                FeeType = feeType,
                WalletPassword = password,
                Recipients = new[] { new Recipient { Amount = amount, ScriptPubKey = destinationScript } }.ToList()
            };
        }

        private static Result<WalletSendTransactionModel> SendManyUtxosTransaction(CoreNode node, Script scriptPubKey, Money amount, int utxos = 1)
        {
            Recipient[] recipients = new Recipient[utxos];
            for (int i = 0; i < recipients.Length; i++)
            {
                recipients[i] = new Recipient { Amount = amount, ScriptPubKey = scriptPubKey };
            }

            var txBuildContext = new TransactionBuildContext(node.FullNode.Network)
            {
                AccountReference = new WalletAccountReference(WalletName, "account 0"),
                MinConfirmations = 1,
                FeeType = FeeType.Medium,
                WalletPassword = Password,
                Recipients = recipients.ToList()
            };

            Transaction trx = (node.FullNode.NodeService<IWalletTransactionHandler>() as IWalletTransactionHandler).BuildTransaction(txBuildContext);

            // Broadcast to the other node.

            IActionResult result = node.FullNode.NodeController<WalletController>()
                .SendTransaction(new SendTransactionRequest(trx.ToHex())).GetAwaiter().GetResult();
            if (result is ErrorResult errorResult)
            {
                var errorResponse = (ErrorResponse)errorResult.Value;
                return Result.Fail<WalletSendTransactionModel>(errorResponse.Errors[0].Message);
            }

            JsonResult response = (JsonResult)result;
            return Result.Ok((WalletSendTransactionModel)response.Value);
        }
    }
}