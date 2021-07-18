using System;
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

using XelsDesktopWalletApp;
using XelsDesktopWalletApp.Models.CommonModels;

namespace XelsDesktopWalletApp
{
    public class Program
    {
        private const string MainchainArgument = "-mainchain";
        private const string SidechainArgument = "-sidechain";


        [STAThread]
        public static void Main(string[] args)
        {
           //args = new string[] {"-sidechain" };

            App app = new App();
            CreateShortCut();

            MainAsync(args).Wait(5);

            app.InitializeComponent();
            app.Run();
        }

        public static async Task MainAsync(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "-mainchain" };
            }
            try
            {
                bool isMainchainNode = args.FirstOrDefault(a => a.ToLower() == MainchainArgument) != null;
                bool isSidechainNode = args.FirstOrDefault(a => a.ToLower() == SidechainArgument) != null;
                bool startInDevMode = args.Any(a => a.ToLower().Contains($"-{NodeSettings.DevModeParam}"));

                IFullNode fullNode = null;

                if (startInDevMode)
                {
                    fullNode = BuildDevCCMiningNode(args);
                }
                else
                {
                    if (isSidechainNode == isMainchainNode)
                        throw new ArgumentException($"Gateway node needs to be started specifying either a {SidechainArgument} or a {MainchainArgument} argument");

                    fullNode = isMainchainNode ? BuildXlcNode(args) : BuildCCMiningNode(args);

                    // set the console window title to identify which node this is (for clarity when running Xlc and CC on the same machine)
                    //Console.Title = isMainchainNode ? $"Xlc Full Node {fullNode.Network.NetworkType}" : $"CC Full Node {fullNode.Network.NetworkType}";
                }

                if (fullNode != null)
                    await fullNode.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }

        private static IFullNode BuildCCMiningNode(string[] args)
        {

            URLConfiguration.Chain = args[0];
            URLConfiguration.BaseURL = "http://localhost:37223/api"; //Side Chain Url

            var nodeSettings = new NodeSettings(networksSelector: CCNetwork.NetworksSelector, protocolVersion: ProtocolVersion.CC_VERSION, args: args)
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

        private static IFullNode BuildDevCCMiningNode(string[] args)
        {
            string[] devModeArgs = new[] { "-bootstrap=1", "-dbtype=rocksdb", "-defaultwalletname=CCdev", "-defaultwalletpassword=password" }.Concat(args).ToArray();
            var network = new CCDev();

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

            URLConfiguration.Chain = args[0];
            // TODO: Hardcode -addressindex for better user experience
            URLConfiguration.BaseURL = "http://localhost:37221/api";//Main Chain Url


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

            string[] argumentList = { "-sidechain" };

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
                shortcut.TargetPath = destinationPath + @"\XelsDesktopWalletApp.exe";
                shortcut.Save();
            }
        }
    }
}
