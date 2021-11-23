using Xels.Bitcoin.Features.SignalR.Broadcasters;
using Xels.Bitcoin.Features.SignalR.Events;

namespace Xels.Bitcoin.Features.SignalR
{
    public static class DaemonConfiguration
    {
        private static readonly IClientEvent[] EventsToHandle = new IClientEvent[]
        {
            new BlockConnectedClientEvent(),
            new ReconstructFederationClientEvent(),
            new FullNodeClientEvent(),
            new TransactionReceivedClientEvent(),
            new WalletProcessedTransactionOfInterestClientEvent()
        };

        private static ClientEventBroadcasterSettings Settings = new ClientEventBroadcasterSettings
        {
            BroadcastFrequencySeconds = 5
        };

        public static void ConfigureSignalRForCc(SignalROptions options)
        {
            options.EventsToHandle = EventsToHandle;

            options.ClientEventBroadcasters = new[]
            {
                (Broadcaster: typeof(CcWalletInfoBroadcaster), ClientEventBroadcasterSettings: Settings)
            };
        }

        public static void ConfigureSignalRForXlc(SignalROptions options)
        {
            options.EventsToHandle = EventsToHandle;

            options.ClientEventBroadcasters = new[]
            {
                (Broadcaster: typeof(StakingBroadcaster), ClientEventBroadcasterSettings: Settings),
                (Broadcaster: typeof(WalletInfoBroadcaster), ClientEventBroadcasterSettings:Settings)
            };
        }

        public static void ConfigureSignalRForPch(SignalROptions options)
        {
            options.EventsToHandle = EventsToHandle;

            options.ClientEventBroadcasters = new[]
            {
                (Broadcaster: typeof(StakingBroadcaster), ClientEventBroadcasterSettings: Settings),
                (Broadcaster: typeof(WalletInfoBroadcaster), ClientEventBroadcasterSettings:Settings)
            };
        }
    }
}
