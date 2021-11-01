using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xels.Bitcoin.Builder;
using Xels.Bitcoin.Builder.Feature;
using Xels.Bitcoin.Configuration.Logging;

namespace Xels.Bitcoin.Features.ExternalApi
{
    public sealed class ExternalApiFeature : FullNodeFeature
    {
        private readonly IExternalApiPoller externalApiPoller;
        
        public ExternalApiFeature(IExternalApiPoller externalApiPoller)
        {
            this.externalApiPoller = externalApiPoller;
        }

        public override Task InitializeAsync()
        {
            this.externalApiPoller?.Initialize();

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            this.externalApiPoller?.Dispose();
        }
    }

    public static partial class IFullNodeBuilderExtensions
    {
        public static IFullNodeBuilder AddExternalApi(this IFullNodeBuilder fullNodeBuilder)
        {
            LoggingConfiguration.RegisterFeatureNamespace<ExternalApiFeature>("externalapi");

            fullNodeBuilder.ConfigureFeature(features =>
                features
                    .AddFeature<ExternalApiFeature>()
                    .FeatureServices(services => services
                    .AddSingleton<ExternalApiSettings>()
                    .AddSingleton<IExternalApiPoller, ExternalApiPoller>()
                    ));

            return fullNodeBuilder;
        }
    }
}
