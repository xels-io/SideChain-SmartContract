using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using Xels.Bitcoin.Networks;

namespace SwapExtractionTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int XelsNetworkApiPort;
            int startBlock = 0;
            Network xlcNetwork;
            string blockExplorerBaseUrl;

            if (args.Contains("-testnet"))
            {
                startBlock = 1528858;

                XelsNetworkApiPort = 38221;
                xlcNetwork = new XlcTest();
                blockExplorerBaseUrl = "https://Xelstestindexer1.azurewebsites.net/api/v1/";
            }
            else
            {
                startBlock = 1975500;

                XelsNetworkApiPort = 37221;
                xlcNetwork = new XlcMain();
                blockExplorerBaseUrl = "https://Xelsmainindexer1.azurewebsites.net/api/v1/";
            }

            var arg = args.FirstOrDefault(a => a.StartsWith("-startfrom"));
            if (arg != null)
                int.TryParse(arg.Split('=')[1], out startBlock);

            if (args.Contains("-swap"))
            {
                var service = new SwapExtractionService(XelsNetworkApiPort, xlcNetwork);
                await service.RunAsync(startBlock, true, false);
            }

            if (args.Contains("-swapvote") || args.Contains("-collateralvote"))
            {
                var blockExplorerClient = new BlockExplorerClient(blockExplorerBaseUrl);
                var service = new VoteExtractionService(XelsNetworkApiPort, xlcNetwork, blockExplorerClient);

                if (args.Contains("-collateralvote"))
                    await service.RunAsync(VoteType.CollateralVote, startBlock);

                if (args.Contains("-swapvote"))
                    await service.RunAsync(VoteType.SwapVote, startBlock);
            }
        }
    }
}
