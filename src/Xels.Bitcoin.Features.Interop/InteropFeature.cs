using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using Xels.Bitcoin.Builder;
using Xels.Bitcoin.Builder.Feature;
using Xels.Bitcoin.Configuration.Logging;
using Xels.Bitcoin.Connection;
using Xels.Bitcoin.Features.Interop.ETHClient;
using Xels.Bitcoin.Features.Interop.Payloads;
using Xels.Bitcoin.Features.PoA;
using Xels.Bitcoin.P2P.Peer;
using Xels.Bitcoin.P2P.Protocol.Payloads;

namespace Xels.Bitcoin.Features.Interop
{
    public sealed class InteropFeature : FullNodeFeature
    {
        private readonly Network network;

        private readonly IFederationManager federationManager;

        private readonly IConnectionManager connectionManager;
        
        private readonly InteropPoller interopPoller;
        
        private readonly IInteropTransactionManager interopTransactionManager;

        private readonly IETHCompatibleClientProvider clientProvider;

        public InteropFeature(
            Network network, 
            IFederationManager federationManager,
            IConnectionManager connectionManager,
            InteropPoller interopPoller,
            IInteropTransactionManager interopTransactionManager,
            IFullNode fullNode,
            IETHCompatibleClientProvider ethCompatibleClientProvider)
        {
            this.network = network;
            this.federationManager = federationManager;
            this.connectionManager = connectionManager;
            this.interopPoller = interopPoller;
            this.interopTransactionManager = interopTransactionManager;
            this.clientProvider = ethCompatibleClientProvider;

            var payloadProvider = (PayloadProvider)fullNode.Services.ServiceProvider.GetService(typeof(PayloadProvider));
            payloadProvider.AddPayload(typeof(InteropCoordinationPayload));
        }

        public override Task InitializeAsync()
        {
            this.interopPoller?.Initialize();

            NetworkPeerConnectionParameters networkPeerConnectionParameters = this.connectionManager.Parameters;
            networkPeerConnectionParameters.TemplateBehaviors.Add(new InteropBehavior(this.network, this.federationManager, this.interopTransactionManager, this.clientProvider));

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            this.interopPoller?.Dispose();
        }
    }

    public static partial class IFullNodeBuilderExtensions
    {
        public static IFullNodeBuilder AddInteroperability(this IFullNodeBuilder fullNodeBuilder)
        {
            LoggingConfiguration.RegisterFeatureNamespace<InteropFeature>("interop");

            fullNodeBuilder.ConfigureFeature(features =>
                features
                    .AddFeature<InteropFeature>()
                    .FeatureServices(services => services
                    .AddSingleton<InteropSettings>()
                    .AddSingleton<IETHClient, ETHClient.ETHClient>()
                    .AddSingleton<IBNBClient, BNBClient>()
                    .AddSingleton<IETHCompatibleClientProvider, ETHCompatibleClientProvider>()
                    .AddSingleton<IInteropTransactionManager, InteropTransactionManager>()
                    .AddSingleton<InteropPoller>()
                    ));

            return fullNodeBuilder;
        }
    }
}
