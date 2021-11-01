﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xels.Bitcoin.Builder;
using Xels.Bitcoin.Builder.Feature;
using Xels.Bitcoin.Features.Miner;
using Xels.Bitcoin.Features.PoA;
using Xels.Bitcoin.Features.SmartContracts;
using Xels.Features.Collateral.CounterChain;

namespace Xels.Features.Collateral
{
    /// <summary>
    /// Sets up the necessary components to check the collateral requirement is met on the counter chain.
    /// </summary>
    public class CollateralFeature : FullNodeFeature
    {
        private readonly ICollateralChecker collateralChecker;

        public CollateralFeature(ICollateralChecker collateralChecker)
        {
            this.collateralChecker = collateralChecker;
        }

        public override async Task InitializeAsync()
        {
            // Note that the node's startup can remain here for a while as it retrieves the collateral for all federation members.
            // This is in contrast with other features' async startup methods that are not required to complete before proceeding.
            await this.collateralChecker.InitializeAsync().ConfigureAwait(false);
        }

        public override void Dispose()
        {
            this.collateralChecker?.Dispose();
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static class FullNodeBuilderCollateralFeatureExtension
    {
        // All Cc nodes should call this.
        public static IFullNodeBuilder CheckCollateralCommitment(this IFullNodeBuilder fullNodeBuilder)
        {
            // These rules always execute between all Cc nodes.
            fullNodeBuilder.Network.Consensus.ConsensusRules.FullValidationRules.Insert(0, typeof(CheckCollateralCommitmentHeightRule));
            return fullNodeBuilder;
        }

        /// <summary>
        /// Adds mining to the side chain node when on a proof-of-authority network with collateral enabled.
        /// </summary>
        public static IFullNodeBuilder AddPoACollateralMiningCapability<T>(this IFullNodeBuilder fullNodeBuilder) where T : BlockDefinition
        {
            // Inject the CheckCollateralFullValidationRule as the first Full Validation Rule.
            // This is still a bit hacky and we need to properly review the dependencies again between the different side chain nodes.
            fullNodeBuilder.Network.Consensus.ConsensusRules.FullValidationRules.Insert(0, typeof(CheckCollateralFullValidationRule));

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<CollateralFeature>()
                .DependOn<CounterChainFeature>()
                .DependOn<PoAFeature>()
                .FeatureServices(services =>
                {
                    services.AddSingleton<IPoAMiner, CollateralPoAMiner>();
                    services.AddSingleton<MinerSettings>();
                    services.AddSingleton<BlockDefinition, T>();
                    services.AddSingleton<IBlockBufferGenerator, BlockBufferGenerator>();

                    services.AddSingleton<ICollateralChecker, CollateralChecker>();
                });
            });

            return fullNodeBuilder;
        }
    }
}
