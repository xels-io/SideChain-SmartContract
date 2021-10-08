﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using IWshRuntimeLibrary;

using NBitcoin.Protocol;
using Xels.Bitcoin;
using Xels.Bitcoin.Builder;
using Xels.Bitcoin.Configuration;
using Xels.Bitcoin.Consensus;
using Xels.Bitcoin.Features.Api;
using Xels.Bitcoin.Features.BlockStore;
using Xels.Bitcoin.Features.Consensus;
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
using Xels.Features.SQLiteWalletRepository;
using Xels.Sidechains.Networks;

namespace Xels.CcMinerD
{
    class Program
    {
        private const string MainchainArgument = "-mainchain";
        private const string SidechainArgument = "-sidechain";

        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
            CreateShortCut();
        }

        public static async Task MainAsync(string[] args)
        {
            try
            {
                bool isMainchainNode = args.FirstOrDefault(a => a.ToLower() == MainchainArgument) != null;
                bool isSidechainNode = args.FirstOrDefault(a => a.ToLower() == SidechainArgument) != null;
                bool startInDevMode = args.Any(a => a.ToLower().Contains($"-{NodeSettings.DevModeParam}"));

                IFullNode fullNode = null;

                if (startInDevMode)
                {
                    fullNode = BuildDevCcMiningNode(args);
                }
                else
                {
                    if (isSidechainNode == isMainchainNode)
                        throw new ArgumentException($"Gateway node needs to be started specifying either a {SidechainArgument} or a {MainchainArgument} argument");

                    fullNode = isMainchainNode ? BuildXlcNode(args) : BuildCcMiningNode(args);

                    // set the console window title to identify which node this is (for clarity when running Xlc and CC on the same machine)
                    Console.Title = isMainchainNode ? $"Xlc Full Node {fullNode.Network.NetworkType}" : $"CC Full Node {fullNode.Network.NetworkType}";
                }

                if (fullNode != null)
                    await fullNode.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }

        private static IFullNode BuildCcMiningNode(string[] args)
        {
            var nodeSettings = new NodeSettings(networksSelector: CcNetwork.NetworksSelector, protocolVersion: ProtocolVersion.CC_VERSION, args: args)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };

            DbType dbType = nodeSettings.GetDbType();

            IFullNode node = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
                .AddPoAFeature()
                .UsePoAConsensus(dbType)
                .AddPoACollateralMiningCapability<SmartContractPoABlockDefinition>()
                .CheckCollateralCommitment()
                .AddDynamicMemberhip()
                .SetCounterChainNetwork(XlcNetwork.MainChainNetworks[nodeSettings.Network.NetworkType]())
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
                .UseSmartContractWallet()
                .AddSQLiteWalletRepository()
                .Build();

            return node;
        }

        private static IFullNode BuildDevCcMiningNode(string[] args)
        {
            string[] devModeArgs = new[] { "-bootstrap=1", "-dbtype=rocksdb", "-defaultwalletname=ccdev", "-defaultwalletpassword=password" }.Concat(args).ToArray();
            var network = new CcDev();

            var nodeSettings = new NodeSettings(network, protocolVersion: ProtocolVersion.CC_VERSION, args: devModeArgs)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };

            DbType dbType = nodeSettings.GetDbType();

            IFullNode node = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
                .AddPoAFeature()
                .UsePoAConsensus(dbType)
                .AddPoAMiningCapability<SmartContractPoABlockDefinition>()
                .UseTransactionNotification()
                .UseBlockNotification()
                .UseApi()
                .UseMempool()
                .AddRPC()
                .AddSmartContracts(options =>
                {
                    options.UseReflectionExecutor();
                    options.UsePoAWhitelistedContracts(true);
                })
                .UseSmartContractWallet()
                .AddSQLiteWalletRepository()
                .Build();

            return node;
        }

        /// <summary>
        /// Returns a standard Xels node. Just like XelsD.
        /// </summary>
        private static IFullNode BuildXlcNode(string[] args)
        {
            // TODO: Hardcode -addressindex for better user experience

            var nodeSettings = new NodeSettings(networksSelector: Networks.Xlc, protocolVersion: ProtocolVersion.PROVEN_HEADER_VERSION, args: args)
            {
                MinProtocolVersion = ProtocolVersion.ALT_PROTOCOL_VERSION
            };




            DbType dbType = nodeSettings.GetDbType();

            IFullNode node = new FullNodeBuilder()
                .UseNodeSettings(nodeSettings, dbType)
                .UseBlockStore(dbType)
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

                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\xels-miner" + arg + ".lnk";
                if (!System.IO.File.Exists(shortcutAddress))
                {
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);

                    shortcut.Arguments = arg;
                    shortcut.TargetPath = destinationPath + @"\Xels.CcMinerD.exe";
                    shortcut.Save();
                }
            }
        }
    }
}
