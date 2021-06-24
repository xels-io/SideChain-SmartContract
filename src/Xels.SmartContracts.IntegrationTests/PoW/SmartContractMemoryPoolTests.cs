using System.Linq;
using System.Threading;
using NBitcoin;
using Xels.Bitcoin.Features.SmartContracts;
using Xels.Bitcoin.Features.SmartContracts.ReflectionExecutor.Consensus.Rules;
using Xels.Bitcoin.IntegrationTests.Common;
using Xels.Bitcoin.Tests.Common;
using Xels.SmartContracts.CLR;
using Xels.SmartContracts.CLR.Serialization;
using Xels.SmartContracts.Core.State;
using Xels.SmartContracts.Tests.Common;
using Xunit;

namespace Xels.SmartContracts.IntegrationTests.PoW
{
    public class SmartContractMemoryPoolTests
    {
        [Fact]
        public void SmartContracts_AddToMempool_Success()
        {
            using (SmartContractNodeBuilder builder = SmartContractNodeBuilder.Create(this))
            {
                var XelsNodeSync = builder.CreateSmartContractPowNode().WithWallet().Start();

                TestHelper.MineBlocks(XelsNodeSync, 2); // coinbase maturity = 0 for this network

                var block = XelsNodeSync.FullNode.BlockStore().GetBlock(XelsNodeSync.FullNode.ChainIndexer.GetHeader(1).HashBlock);
                var prevTrx = block.Transactions.First();
                var dest = new BitcoinSecret(new Key(), XelsNodeSync.FullNode.Network);

                var tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(XelsNodeSync.MinerSecret.PubKey)));
                tx.AddOutput(new TxOut("25", dest.PubKey.Hash));
                tx.AddOutput(new TxOut("24", new Key().PubKey.Hash)); // 1 btc fee
                tx.Sign(XelsNodeSync.FullNode.Network, XelsNodeSync.MinerSecret, false);

                XelsNodeSync.Broadcast(tx);

                TestBase.WaitLoop(() => XelsNodeSync.CreateRPCClient().GetRawMempool().Length == 1);
            }
        }

        [Fact]
        public void SmartContracts_AddToMempool_OnlyValid()
        {
            using (SmartContractNodeBuilder builder = SmartContractNodeBuilder.Create(this))
            {
                var XelsNodeSync = builder.CreateSmartContractPowNode().WithWallet().Start();

                var callDataSerializer = new CallDataSerializer(new ContractPrimitiveSerializer(XelsNodeSync.FullNode.Network));

                TestHelper.MineBlocks(XelsNodeSync, 2); // coinbase maturity = 0 for this network

                var block = XelsNodeSync.FullNode.BlockStore().GetBlock(XelsNodeSync.FullNode.ChainIndexer.GetHeader(1).HashBlock);
                var prevTrx = block.Transactions.First();
                var dest = new BitcoinSecret(new Key(), XelsNodeSync.FullNode.Network);

                // Gas higher than allowed limit
                var tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(XelsNodeSync.MinerSecret.PubKey)));
                var contractTxData = new ContractTxData(1, 100, new RuntimeObserver.Gas(10_000_000), new uint160(0), "Test");
                tx.AddOutput(new TxOut(1, new Script(callDataSerializer.Serialize(contractTxData))));
                tx.Sign(XelsNodeSync.FullNode.Network, XelsNodeSync.MinerSecret, false);
                XelsNodeSync.Broadcast(tx);

                // OP_SPEND in user's tx - we can't sign this because the TransactionBuilder recognises the ScriptPubKey is invalid.
                tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), new Script(new[] { (byte)ScOpcodeType.OP_SPEND })));
                tx.AddOutput(new TxOut(1, new Script(callDataSerializer.Serialize(contractTxData))));
                XelsNodeSync.Broadcast(tx);

                // 2 smart contract outputs
                tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(XelsNodeSync.MinerSecret.PubKey)));
                tx.AddOutput(new TxOut(1, new Script(callDataSerializer.Serialize(contractTxData))));
                tx.AddOutput(new TxOut(1, new Script(callDataSerializer.Serialize(contractTxData))));
                tx.Sign(XelsNodeSync.FullNode.Network, XelsNodeSync.MinerSecret, false);
                XelsNodeSync.Broadcast(tx);

                // Send to contract
                uint160 contractAddress = new uint160(123);
                var state = XelsNodeSync.FullNode.NodeService<IStateRepositoryRoot>();
                state.CreateAccount(contractAddress);
                state.Commit();
                tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(XelsNodeSync.MinerSecret.PubKey)));
                tx.AddOutput(new TxOut(100, PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(new KeyId(contractAddress))));
                tx.Sign(XelsNodeSync.FullNode.Network, XelsNodeSync.MinerSecret, false);
                XelsNodeSync.Broadcast(tx);

                // Gas price lower than minimum
                tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(XelsNodeSync.MinerSecret.PubKey)));
                var lowGasPriceContractTxData = new ContractTxData(1, SmartContractMempoolValidator.MinGasPrice - 1, new RuntimeObserver.Gas(SmartContractFormatLogic.GasLimitMaximum), new uint160(0), "Test");
                tx.AddOutput(new TxOut(1, new Script(callDataSerializer.Serialize(lowGasPriceContractTxData))));
                tx.Sign(XelsNodeSync.FullNode.Network, XelsNodeSync.MinerSecret, false);
                XelsNodeSync.Broadcast(tx);

                // After 5 seconds (plenty of time but ideally we would have a more accurate measure) no txs in mempool. All failed validation.
                Thread.Sleep(5000);
                Assert.Empty(XelsNodeSync.CreateRPCClient().GetRawMempool());

                // Valid tx still works
                tx = XelsNodeSync.FullNode.Network.CreateTransaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(XelsNodeSync.MinerSecret.PubKey)));
                tx.AddOutput(new TxOut("25", dest.PubKey.Hash));
                tx.AddOutput(new TxOut("24", new Key().PubKey.Hash)); // 1 btc fee
                tx.Sign(XelsNodeSync.FullNode.Network, XelsNodeSync.MinerSecret, false);
                XelsNodeSync.Broadcast(tx);
                TestBase.WaitLoop(() => XelsNodeSync.CreateRPCClient().GetRawMempool().Length == 1);
            }
        }
    }
}