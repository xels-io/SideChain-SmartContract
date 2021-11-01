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
            int xelsNetworkApiPort;
            int startBlock = 0;
            Network xlcNetwork;
            string blockExplorerBaseUrl;

            if (args.Contains("-testnet"))
            {
                startBlock = 1528858;

                xelsNetworkApiPort = 38221;
                xlcNetwork = new XlcTest();
                blockExplorerBaseUrl = "https://xelstestindexer1.azurewebsites.net/api/v1/";
            }
            else
            {
                startBlock = 1975500;

                xelsNetworkApiPort = 37221;
                xlcNetwork = new XlcMain();
                blockExplorerBaseUrl = "https://xelsmainindexer1.azurewebsites.net/api/v1/";
            }

            var arg = args.FirstOrDefault(a => a.StartsWith("-startfrom"));
            if (arg != null)
                int.TryParse(arg.Split('=')[1], out startBlock);

            if (args.Contains("-swap"))
            {
                var service = new SwapExtractionService(xelsNetworkApiPort, xlcNetwork);
                await service.RunAsync(startBlock, true, false);
            }

            if (args.Contains("-swapvote") || args.Contains("-collateralvote"))
            {
                var blockExplorerClient = new BlockExplorerClient(blockExplorerBaseUrl);
                var service = new VoteExtractionService(xelsNetworkApiPort, xlcNetwork, blockExplorerClient);

                if (args.Contains("-collateralvote"))
                    await service.RunAsync(VoteType.CollateralVote, startBlock);

                if (args.Contains("-swapvote"))
                    await service.RunAsync(VoteType.SwapVote, startBlock);
            }
        }
    }
}
