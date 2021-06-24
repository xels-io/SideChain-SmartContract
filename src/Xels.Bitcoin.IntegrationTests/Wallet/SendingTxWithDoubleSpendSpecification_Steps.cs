using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NBitcoin;
using Xels.Bitcoin.Features.MemoryPool;
using Xels.Bitcoin.Features.Wallet;
using Xels.Bitcoin.Features.Wallet.Controllers;
using Xels.Bitcoin.Features.Wallet.Models;
using Xels.Bitcoin.IntegrationTests.Common;
using Xels.Bitcoin.IntegrationTests.Common.EnvironmentMockUpHelpers;
using Xels.Bitcoin.IntegrationTests.Common.ReadyData;
using Xels.Bitcoin.Networks;
using Xels.Bitcoin.Tests.Common;
using Xels.Bitcoin.Tests.Common.TestFramework;
using Xunit.Abstractions;

namespace Xels.Bitcoin.IntegrationTests.Wallet
{
    public partial class SendingTransactionWithDoubleSpend : BddSpecification
    {
        private const string Password = "password";
        private const string Name = "mywallet";
        private const string Passphrase = "passphrase";
        private const string AccountName = "account 0";

        private NodeBuilder builder;
        private Network network;
        private CoreNode XelsSender;
        private CoreNode XelsReceiver;
        private Transaction transaction;
        private MempoolValidationState mempoolValidationState;
        private HdAddress receivingAddress;

        public SendingTransactionWithDoubleSpend(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override void BeforeTest()
        {
            this.builder = NodeBuilder.Create(this);
            this.network = new BitcoinRegTest();

            this.XelsSender = this.builder.CreateXelsPowNode(this.network).WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest100Miner).Start();
            this.XelsReceiver = this.builder.CreateXelsPowNode(this.network).WithWallet().Start();
            this.mempoolValidationState = new MempoolValidationState(true);
        }

        protected override void AfterTest()
        {
            this.builder.Dispose();
        }

        private void wallets_with_coins()
        {
            var maturity = (int)this.XelsSender.FullNode.Network.Consensus.CoinbaseMaturity;
            TestHelper.MineBlocks(this.XelsSender, 5);

            var total = this.XelsSender.FullNode.WalletManager().GetSpendableTransactionsInWallet(Name).Sum(s => s.Transaction.Amount);
            total.Should().Equals(Money.COIN * 6 * 50);

            TestHelper.ConnectAndSync(this.XelsSender, this.XelsReceiver);
        }

        private void coins_first_sent_to_receiving_wallet()
        {
            this.receivingAddress = this.XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(Name, AccountName));

            this.transaction = this.XelsSender.FullNode.WalletTransactionHandler().BuildTransaction(WalletTests.CreateContext(this.XelsSender.FullNode.Network,
                new WalletAccountReference(Name, AccountName), Password, this.receivingAddress.ScriptPubKey, Money.COIN * 100, FeeType.Medium, 101));

            this.XelsSender.FullNode.NodeController<WalletController>().SendTransaction(new SendTransactionRequest(this.transaction.ToHex()));

            TestBase.WaitLoop(() => this.XelsReceiver.CreateRPCClient().GetRawMempool().Length > 0);
            TestBase.WaitLoop(() => this.XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(Name).Any());

            var receivetotal = this.XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(Name).Sum(s => s.Transaction.Amount);
            receivetotal.Should().Equals(Money.COIN * 100);
            this.XelsReceiver.FullNode.WalletManager().GetSpendableTransactionsInWallet(Name).First().Transaction.BlockHeight.Should().BeNull();
        }

        private void txn_mempool_conflict_error_occurs()
        {
            this.mempoolValidationState.Error.Code.Should().BeEquivalentTo("txn-mempool-conflict");
        }

        private void receiving_node_attempts_to_double_spend_mempool_doesnotaccept()
        {
            var unusedAddress = this.XelsReceiver.FullNode.WalletManager().GetUnusedAddress(new WalletAccountReference(Name, AccountName));
            var transactionCloned = this.XelsReceiver.FullNode.Network.CreateTransaction(this.transaction.ToBytes());
            transactionCloned.Outputs[1].ScriptPubKey = unusedAddress.ScriptPubKey;
            this.XelsReceiver.FullNode.MempoolManager().Validator.AcceptToMemoryPool(this.mempoolValidationState, transactionCloned).Result.Should().BeFalse();
        }

        private void trx_is_mined_into_a_block_and_removed_from_mempools()
        {
            TestHelper.MineBlocks(this.XelsSender, 1);
            TestHelper.WaitForNodeToSync(this.XelsSender, this.XelsReceiver);

            this.XelsSender.FullNode.MempoolManager().GetMempoolAsync().Result.Should().NotContain(this.transaction.GetHash());
            this.XelsReceiver.FullNode.MempoolManager().GetMempoolAsync().Result.Should().NotContain(this.transaction.GetHash());
        }

        private void trx_is_propagated_across_sending_and_receiving_mempools()
        {
            List<uint256> senderMempoolTransactions = this.XelsSender.FullNode.MempoolManager().GetMempoolAsync().Result;
            senderMempoolTransactions.Should().Contain(this.transaction.GetHash());

            List<uint256> receiverMempoolTransactions = this.XelsSender.FullNode.MempoolManager().GetMempoolAsync().Result;
            receiverMempoolTransactions.Should().Contain(this.transaction.GetHash());
        }
    }
}