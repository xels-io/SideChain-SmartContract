﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
using XelsPCHDesktopWalletApp.Common;
using XelsPCHDesktopWalletApp.Models.CommonModels;

namespace XelsPCHDesktopWalletApp
{
    public class Program
    {
        private const string PchchainArgument = "-pchchain";

        [STAThread]
        public static void Main(string[] args)
        {
            args = new string[] { PchchainArgument };

            App app = new App();
            CreateShortCut();

            MainAsync(args).Wait(5);

            app.InitializeComponent();
            app.Run();
        }
        public static async Task MainAsync(string[] args)
        {
            try
            {
                IFullNode fullNode = BuildPchNode(args);

                if (fullNode != null)
                    await fullNode.RunAsync();
            }
            catch (Exception ex)
            {
                GlobalExceptionHandler.SendErrorToText(ex);
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }

        private static IFullNode BuildPchNode(string[] args)
        {

            URLConfiguration.Chain = args[0];
            // TODO: Hardcode -addressindex for better user experience
            URLConfiguration.BaseURL = "http://localhost:38221/api";//Main Chain Url


            // TODO: Hardcode -addressindex for better user experience

            var nodeSettings = new NodeSettings(networksSelector: Networks.Pch, protocolVersion: ProtocolVersion.PROVEN_HEADER_VERSION, args: args)
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

            string[] argumentList = { PchchainArgument };

            string destinationPath = Directory.GetCurrentDirectory();

            //Console.WriteLine(distinationPath);
            //Console.ReadLine();
            foreach (var arg in argumentList)
            {

                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();

                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\xels-app" + arg + ".lnk";
                if (!System.IO.File.Exists(shortcutAddress))
                {
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);

                    shortcut.Arguments = arg;
                    shortcut.TargetPath = destinationPath + @"\XelsPCHDesktopWalletApp.exe";
                    shortcut.Save();
                }
            }
        }
    }
}
