using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using Xels.Bitcoin.Features.BlockStore;
using Xels.Bitcoin.Features.BlockStore.Repositories;
using Xels.Bitcoin.IntegrationTests.Common;
using Xels.Bitcoin.IntegrationTests.Common.EnvironmentMockUpHelpers;
using Xels.Bitcoin.IntegrationTests.Common.ReadyData;
using Xels.Bitcoin.Networks;
using Xels.Bitcoin.Tests.Common;
using Xels.Bitcoin.Utilities;
using Xunit;

namespace Xels.Bitcoin.IntegrationTests.BlockStore
{
    public class BlockStoreTests
    {
        private readonly Network network;

        public BlockStoreTests()
        {
            this.network = new BitcoinRegTest();
        }

        [Fact]
        public void BlockRepositoryPutBatch()
        {
            var dBreezeSerializer = new DBreezeSerializer(this.network.Consensus.ConsensusFactory);

            using (var blockRepository = new LevelDbBlockRepository(this.network, TestBase.CreateDataFolder(this), dBreezeSerializer))
            {
                blockRepository.Initialize();

                blockRepository.SetTxIndex(true);

                var blocks = new List<Block>();
                for (int i = 0; i < 5; i++)
                {
                    Block block = this.network.CreateBlock();
                    block.AddTransaction(this.network.CreateTransaction());
                    block.AddTransaction(this.network.CreateTransaction());
                    block.Transactions[0].AddInput(new TxIn(Script.Empty));
                    block.Transactions[0].AddOutput(Money.COIN + i * 2, Script.Empty);
                    block.Transactions[1].AddInput(new TxIn(Script.Empty));
                    block.Transactions[1].AddOutput(Money.COIN + i * 2 + 1, Script.Empty);
                    block.UpdateMerkleRoot();
                    block.Header.HashPrevBlock = blocks.Any() ? blocks.Last().GetHash() : this.network.GenesisHash;
                    blocks.Add(block);
                }

                // put
                blockRepository.PutBlocks(new HashHeightPair(blocks.Last().GetHash(), blocks.Count), blocks);

                // check the presence of each block in the repository
                foreach (Block block in blocks)
                {
                    Block received = blockRepository.GetBlock(block.GetHash());
                    Assert.True(block.ToBytes().SequenceEqual(received.ToBytes()));

                    foreach (Transaction transaction in block.Transactions)
                    {
                        Transaction trx = blockRepository.GetTransactionById(transaction.GetHash());
                        Assert.True(trx.ToBytes().SequenceEqual(transaction.ToBytes()));
                    }
                }

                // delete
                blockRepository.Delete(new HashHeightPair(blocks.ElementAt(2).GetHash(), 2), new[] { blocks.ElementAt(2).GetHash() }.ToList());
                Block deleted = blockRepository.GetBlock(blocks.ElementAt(2).GetHash());
                Assert.Null(deleted);
            }
        }

        [Fact]
        public void BlockBroadcastInv()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsNodeSync = builder.CreateXelsPowNode(this.network, "bs-1-XelsNodeSync").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();
                CoreNode XelsNode1 = builder.CreateXelsPowNode(this.network, "bs-1-XelsNode1").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10NoWallet).Start();
                CoreNode XelsNode2 = builder.CreateXelsPowNode(this.network, "bs-1-XelsNode2").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10NoWallet).Start();

                // Sync both nodes
                TestHelper.ConnectAndSync(XelsNode1, XelsNodeSync);
                TestHelper.ConnectAndSync(XelsNode2, XelsNodeSync);

                // Set node2 to use inv (not headers).
                XelsNode2.FullNode.ConnectionManager.ConnectedPeers.First().Behavior<BlockStoreBehavior>().PreferHeaders = false;

                // Generate two new blocks.
                TestHelper.MineBlocks(XelsNodeSync, 2);

                // Wait for the other nodes to pick up the newly generated blocks
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsNode1, XelsNodeSync));
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsNode2, XelsNodeSync));
            }
        }

        [Fact(Skip = "Investigate PeerConnector shutdown timeout issue")]
        public void BlockStoreCanRecoverOnStartup()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsNodeSync = builder.CreateXelsPowNode(this.network, "bs-2-XelsNodeSync").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();

                // Set the tip of the best chain to some blocks in the past.
                XelsNodeSync.FullNode.ChainIndexer.SetTip(XelsNodeSync.FullNode.ChainIndexer.GetHeader(XelsNodeSync.FullNode.ChainIndexer.Height - 5));

                // Stop the node to persist the chain with the reset tip.
                XelsNodeSync.FullNode.Dispose();

                CoreNode newNodeInstance = builder.CloneXelsNode(XelsNodeSync);

                // Start the node, this should hit the block store recover code.
                newNodeInstance.Start();

                // Check that the store recovered to be the same as the best chain.
                Assert.Equal(newNodeInstance.FullNode.ChainIndexer.Tip.HashBlock, newNodeInstance.FullNode.GetBlockStoreTip().HashBlock);
            }
        }

        [Fact]
        public void BlockStoreCanReorg()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsNodeSync = builder.CreateXelsPowNode(this.network, "bs-3-XelsNodeSync").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();
                CoreNode XelsNode1 = builder.CreateXelsPowNode(this.network, "bs-3-XelsNode1").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Listener).Start();
                CoreNode XelsNode2 = builder.CreateXelsPowNode(this.network, "bs-3-XelsNode2").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Listener).Start();

                // Sync both nodes.
                TestHelper.ConnectAndSync(XelsNodeSync, XelsNode1);
                TestHelper.ConnectAndSync(XelsNodeSync, XelsNode2);

                // Remove node 2.
                TestHelper.Disconnect(XelsNodeSync, XelsNode2);

                // Mine some more with node 1
                TestHelper.MineBlocks(XelsNode1, 10);

                // Wait for node 1 to sync
                TestBase.WaitLoop(() => XelsNode1.FullNode.GetBlockStoreTip().Height == 20);
                TestBase.WaitLoop(() => XelsNode1.FullNode.GetBlockStoreTip().HashBlock == XelsNodeSync.FullNode.GetBlockStoreTip().HashBlock);

                // Remove node 1.
                TestHelper.Disconnect(XelsNodeSync, XelsNode1);

                // Mine a higher chain with node 2.
                TestHelper.MineBlocks(XelsNode2, 20);
                TestBase.WaitLoop(() => XelsNode2.FullNode.GetBlockStoreTip().Height == 30);

                // Add node 2.
                TestHelper.Connect(XelsNodeSync, XelsNode2);

                // Node2 should be synced.
                TestBase.WaitLoop(() => TestHelper.AreNodesSynced(XelsNode2, XelsNodeSync));
            }
        }

        [Fact]
        public void BlockStoreIndexTx()
        {
            using (NodeBuilder builder = NodeBuilder.Create(this))
            {
                CoreNode XelsNode1 = builder.CreateXelsPowNode(this.network, "bs-4-XelsNode1").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10Miner).Start();
                CoreNode XelsNode2 = builder.CreateXelsPowNode(this.network, "bs-4-XelsNode2").WithReadyBlockchainData(ReadyBlockchain.BitcoinRegTest10NoWallet).Start();

                // Sync both nodes.
                TestHelper.ConnectAndSync(XelsNode1, XelsNode2);

                TestBase.WaitLoop(() => XelsNode1.FullNode.GetBlockStoreTip().Height == 10);
                TestBase.WaitLoop(() => XelsNode1.FullNode.GetBlockStoreTip().HashBlock == XelsNode2.FullNode.GetBlockStoreTip().HashBlock);

                Block bestBlock1 = XelsNode1.FullNode.BlockStore().GetBlock(XelsNode1.FullNode.ChainIndexer.Tip.HashBlock);
                Assert.NotNull(bestBlock1);

                // Get the block coinbase trx.
                Transaction trx = XelsNode2.FullNode.BlockStore().GetTransactionById(bestBlock1.Transactions.First().GetHash());
                Assert.NotNull(trx);
                Assert.Equal(bestBlock1.Transactions.First().GetHash(), trx.GetHash());
            }
        }
    }
}
