using NBitcoin;
using Xels.Bitcoin.Features.SmartContracts.Models;
using Xels.SmartContracts.CLR;
using Xels.SmartContracts.CLR.Compilation;
using Xels.SmartContracts.Core.Util;
using Xels.SmartContracts.RuntimeObserver;
using Xels.SmartContracts.Tests.Common.MockChain;
using Xunit;

namespace Xels.SmartContracts.IntegrationTests
{
    public abstract class ContractBehaviourTests<T> : IClassFixture<T> where T : class, IMockChainFixture
    {
        private readonly IMockChain mockChain;
        private readonly MockChainNode node1;
        private readonly MockChainNode node2;

        private readonly IAddressGenerator addressGenerator;
        private readonly ISenderRetriever senderRetriever;

        protected ContractBehaviourTests(T fixture)
        {
            this.mockChain = fixture.Chain;
            this.node1 = this.mockChain.Nodes[0];
            this.node2 = this.mockChain.Nodes[1];
            this.addressGenerator = new AddressGenerator();
            this.senderRetriever = new SenderRetriever();
        }

        [Fact]
        public void Persisting_Nothing_Null_And_Empty_Byte_Arrays_Are_The_Same()
        {
            // Demonstrates some potentially unusual behaviour when saving contract state.

            // Ensure fixture is funded.
            this.mockChain.MineBlocks(1);

            // Deploy contract
            ContractCompilationResult compilationResult = ContractCompiler.CompileFile("SmartContracts/BehaviourTest.cs");

            Assert.True(compilationResult.Success);
            BuildCreateContractTransactionResponse preResponse = this.node1.SendCreateContractTransaction(compilationResult.Compilation, 0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);
            Assert.NotNull(this.node1.GetCode(preResponse.NewContractAddress));

            uint256 currentHash = this.node1.GetLastBlock().GetHash();

            // Call CheckData() and confirm that it succeeds
            BuildCallContractTransactionResponse response = this.node1.SendCallContractTransaction(
                nameof(BehaviourTest.DataIsByte0),
                preResponse.NewContractAddress,
                0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);

            NBitcoin.Block lastBlock = this.node1.GetLastBlock();

            // Blocks progressed
            Assert.NotEqual(currentHash, lastBlock.GetHash());

            ReceiptResponse receipt = this.node1.GetReceipt(response.TransactionId.ToString());
            Assert.True(receipt.Success);

            // Invoke PersistEmptyString. It should succeed
             response = this.node1.SendCallContractTransaction(
                nameof(BehaviourTest.PersistEmptyString),
                preResponse.NewContractAddress,
                0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);

            lastBlock = this.node1.GetLastBlock();

            // Blocks progressed
            Assert.NotEqual(currentHash, lastBlock.GetHash());

            receipt = this.node1.GetReceipt(response.TransactionId.ToString());
            Assert.True(receipt.Success);

            // The storage value should be null, but the contract only sees it as a byte[0]
            Assert.Null(this.node1.GetStorageValue(preResponse.NewContractAddress, nameof(BehaviourTest.Data)));

            // Now call CheckData() to confirm that it's a byte[0]
            response = this.node1.SendCallContractTransaction(
                nameof(BehaviourTest.DataIsByte0),
                preResponse.NewContractAddress,
                0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);

            lastBlock = this.node1.GetLastBlock();

            // Blocks progressed
            Assert.NotEqual(currentHash, lastBlock.GetHash());
            
            receipt = this.node1.GetReceipt(response.TransactionId.ToString());
            Assert.True(receipt.Success);

            // Now call PersistNull, which should fail if the null is not returned as byte[0]
            response = this.node1.SendCallContractTransaction(
                nameof(BehaviourTest.PersistNull),
                preResponse.NewContractAddress,
                0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);

            lastBlock = this.node1.GetLastBlock();

            receipt = this.node1.GetReceipt(response.TransactionId.ToString());
            Assert.True(receipt.Success);

            // The storage value should be null
            Assert.Null(this.node1.GetStorageValue(preResponse.NewContractAddress, nameof(BehaviourTest.Data)));

            // Now call CheckData() to confirm that it's a byte[0]
            response = this.node1.SendCallContractTransaction(
                nameof(BehaviourTest.DataIsByte0),
                preResponse.NewContractAddress,
                0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);

            lastBlock = this.node1.GetLastBlock();

            // Blocks progressed
            Assert.NotEqual(currentHash, lastBlock.GetHash());

            receipt = this.node1.GetReceipt(response.TransactionId.ToString());
            Assert.True(receipt.Success);
        }

        [Fact]
        public void Local_Call_At_Height()
        {
            // Demonstrates some potentially unusual behaviour when saving contract state.
            var localExecutor = this.mockChain.Nodes[0].CoreNode.FullNode.NodeService<ILocalExecutor>();

            // Ensure fixture is funded.
            this.mockChain.MineBlocks(1);

            // Deploy contract
            ContractCompilationResult compilationResult = ContractCompiler.CompileFile("SmartContracts/StorageDemo.cs");

            Assert.True(compilationResult.Success);
            BuildCreateContractTransactionResponse preResponse = this.node1.SendCreateContractTransaction(compilationResult.Compilation, 0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);
            Assert.NotNull(this.node1.GetCode(preResponse.NewContractAddress));

            uint256 currentHash = this.node1.GetLastBlock().GetHash();

            NBitcoin.Block lastBlock = this.node1.GetLastBlock();

            ReceiptResponse receipt = this.node1.GetReceipt(preResponse.TransactionId.ToString());
            Assert.True(receipt.Success);

            // Get initial state at height
            var call = new ContractTxData(1, 100, (Gas)250000, preResponse.NewContractAddress.ToUint160(this.node1.CoreNode.FullNode.Network), nameof(StorageDemo.GetCounterValue));
            var initialState = localExecutor.Execute((ulong)this.node1.CoreNode.FullNode.ChainIndexer.Height, this.mockChain.Nodes[0].MinerAddress.Address.ToUint160(this.node1.CoreNode.FullNode.Network), 0, call);
            
            Assert.Equal(12345, (int)initialState.Return);

            // Call Counter() and confirm that it succeeds
            BuildCallContractTransactionResponse response = this.node1.SendCallContractTransaction(
                nameof(StorageDemo.Increment),
                preResponse.NewContractAddress,
                0);
            this.mockChain.WaitAllMempoolCount(1);
            this.mockChain.MineBlocks(1);

            lastBlock = this.node1.GetLastBlock();

            // Blocks progressed
            Assert.NotEqual(currentHash, lastBlock.GetHash());

            // Get local state at height 2 after calling counter
            initialState = localExecutor.Execute((ulong)this.node1.CoreNode.FullNode.ChainIndexer.Height, this.mockChain.Nodes[0].MinerAddress.Address.ToUint160(this.node1.CoreNode.FullNode.Network), 0, call);

            Assert.Equal(12346, (int)initialState.Return);

            // Get local state at previous height before calling counter
            initialState = localExecutor.Execute((ulong)this.node1.CoreNode.FullNode.ChainIndexer.Height - 1, this.mockChain.Nodes[0].MinerAddress.Address.ToUint160(this.node1.CoreNode.FullNode.Network), 0, call);

            Assert.Equal(12345, (int)initialState.Return);
        }
    }

    public class PoAContractBehaviourTests : ContractBehaviourTests<PoAMockChainFixture>
    {
        public PoAContractBehaviourTests(PoAMockChainFixture fixture) : base(fixture)
        {
        }
    }

    public class PoWContractBehaviourTests : ContractBehaviourTests<PoWMockChainFixture>
    {
        public PoWContractBehaviourTests(PoWMockChainFixture fixture) : base(fixture)
        {
        }
    }
}
