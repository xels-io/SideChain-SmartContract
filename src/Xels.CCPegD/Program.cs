using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using IWshRuntimeLibrary;

using NBitcoin;
using NBitcoin.Protocol;

using Xels.Bitcoin;
using Xels.Bitcoin.Builder;
using Xels.Bitcoin.Configuration;
using Xels.Bitcoin.Consensus;
using Xels.Bitcoin.Features.Api;
using Xels.Bitcoin.Features.BlockStore;
using Xels.Bitcoin.Features.Consensus;
using Xels.Bitcoin.Features.Interop;
using Xels.Bitcoin.Features.MemoryPool;
using Xels.Bitcoin.Features.Miner;
using Xels.Bitcoin.Features.Notifications;
using Xels.Bitcoin.Features.RPC;
using Xels.Bitcoin.Features.SmartContracts;
using Xels.Bitcoin.Features.SmartContracts.PoA;
using Xels.Bitcoin.Features.SmartContracts.Wallet;
using Xels.Bitcoin.Features.Wallet;
using Xels.Bitcoin.Networks;
using Xels.Bitcoin.Utilities;
using Xels.Features.Collateral;
using Xels.Features.Collateral.CounterChain;
using Xels.Features.FederatedPeg;
using Xels.Features.SQLiteWalletRepository;
using Xels.Sidechains.Networks;

namespace Xels.CCPegD
{
    class Program
    {
        private const string MainchainArgument = "-mainchain";
        private const string SidechainArgument = "-sidechain";

        private static readonly Dictionary<NetworkType, Func<Network>> SidechainNetworks = new Dictionary<NetworkType, Func<Network>>
        {
            { NetworkType.Mainnet, CCNetwork.NetworksSelector.Mainnet },
            { NetworkType.Testnet, CCNetwork.NetworksSelector.Testnet },
            { NetworkType.Regtest, CCNetwork.NetworksSelector.Regtest }
        };

        private static void Main(string[] args)
        {
           //args = new string[] { "-sidechain" };           
            RunFederationGatewayAsync(args).Wait();
            CreateShortCut();
        }

        private static async Task RunFederationGatewayAsync(string[] args)
        {
            try
            {
                bool isMainchainNode = args.FirstOrDefault(a => a.ToLower() == MainchainArgument) != null;
                bool isSidechainNode = args.FirstOrDefault(a => a.ToLower() == SidechainArgument) != null;

                if (isSidechainNode == isMainchainNode)
                {
                    throw new ArgumentException($"Gateway node needs to be started specifying either a {SidechainArgument} or a {MainchainArgument} argument");
                }

                IFullNode node = isMainchainNode ? GetMainchainFullNode(args) : GetSidechainFullNode(args);

                // set the console window title to identify which node this is (for clarity when running Xlc and CC on the same machine)
                Console.Title = isMainchainNode ? $"Xlc Full Node {node.Network.NetworkType}" : $"CC Full Node {node.Network.NetworkType}";

                if (node != null)
                    await node.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }

        private static IFullNode GetMainchainFullNode(string[] args)
        {
            // TODO: Hardcode -addressindex

            var nodeSettings = new NodeSettings(networksSelector: Networks.Xlc, protocolVersion: ProtocolVersion.PROVEN_HEADER_VERSION, args: args)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };

            NetworkType networkType = nodeSettings.Network.NetworkType;

            DbType dbType = nodeSettings.GetDbType();

            IFullNode node = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
                .SetCounterChainNetwork(SidechainNetworks[nodeSettings.Network.NetworkType]())
                .AddFederatedPeg(isMainChain: true)
                .UseTransactionNotification()
                .UseBlockNotification()
                .UseApi()
                .UseMempool()
                .AddRPC()
                .UsePosConsensus(dbType)
                .UseWallet()
                .AddSQLiteWalletRepository()
                .AddPowPosMining(true)
                .Build();

            return node;
        }

        private static IFullNode GetSidechainFullNode(string[] args)
        {
            var nodeSettings = new NodeSettings(networksSelector: CCNetwork.NetworksSelector, protocolVersion: ProtocolVersion.CC_VERSION, args: args)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };

            DbType dbType = nodeSettings.GetDbType();

            IFullNode node = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
                .SetCounterChainNetwork(XlcNetwork.MainChainNetworks[nodeSettings.Network.NetworkType]())
                .AddPoAFeature()
                .UsePoAConsensus(dbType)
                .AddFederatedPeg()
                .AddPoACollateralMiningCapability<FederatedPegBlockDefinition>()
                .CheckCollateralCommitment()
                .AddDynamicMemberhip()
                .UseTransactionNotification()
                .UseBlockNotification()
                .UseApi()
                .UseMempool()
                .AddRPC()
                .AddSmartContracts(options =>
                {
                    options.UseReflectionExecutor();
                    options.UsePoAWhitelistedContracts();
                })
                .AddInteroperability()
                .UseSmartContractWallet()
                .AddSQLiteWalletRepository()
                .Build();

            return node;
        }

        public static void CreateShortCut()
        {

            string[] argumentList = { "-mainchain", "-sidechain" };

            string destinationPath = Directory.GetCurrentDirectory();
            //Console.WriteLine(distinationPath);
            //Console.ReadLine();
            foreach (var arg in argumentList)
            {
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\xels-app" + arg + ".lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);

                shortcut.Arguments = arg;
                shortcut.TargetPath = destinationPath + @"\Xels.CCPegD.exe";
                shortcut.Save();
            }
        }
    }
}
