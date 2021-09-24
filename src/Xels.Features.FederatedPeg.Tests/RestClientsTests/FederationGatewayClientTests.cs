﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xels.Bitcoin.Configuration;
using Xels.Bitcoin.Configuration.Logging;
using Xels.Bitcoin.Controllers;
using Xels.Bitcoin.Networks;
using Xels.Bitcoin.Utilities;
using Xels.Features.Collateral.CounterChain;
using Xels.Features.FederatedPeg.Controllers;
using Xels.Features.FederatedPeg.Models;
using Xunit;

namespace Xels.Features.FederatedPeg.Tests.RestClientsTests
{
    public class FederationGatewayClientTests
    {
        private readonly FederationGatewayClient client;

        public FederationGatewayClientTests()
        {
            string redeemScript = "2 02fad5f3c4fdf4c22e8be4cfda47882fff89aaa0a48c1ccad7fa80dc5fee9ccec3 02503f03243d41c141172465caca2f5cef7524f149e965483be7ce4e44107d7d35 03be943c3a31359cd8e67bedb7122a0898d2c204cf2d0119e923ded58c429ef97c 3 OP_CHECKMULTISIG";
            string federationIps = "127.0.0.1:36201,127.0.0.1:36202,127.0.0.1:36203";
            string multisigPubKey = "03be943c3a31359cd8e67bedb7122a0898d2c204cf2d0119e923ded58c429ef97c";
            string[] args = new[] { "-sidechain", "-regtest", $"-federationips={federationIps}", $"-redeemscript={redeemScript}", $"-publickey={multisigPubKey}", "-mincoinmaturity=1", "-mindepositconfirmations=1" };

            var nodeSettings = new NodeSettings(Sidechains.Networks.CcNetwork.NetworksSelector.Regtest(), NBitcoin.Protocol.ProtocolVersion.ALT_PROTOCOL_VERSION, args: args);

            this.client = new FederationGatewayClient(new CounterChainSettings(nodeSettings, new CounterChainNetworkWrapper(Networks.Xlc.Regtest())), new HttpClientFactory());
        }

        [Fact]
        public async Task ReturnsNullIfCounterChainNodeIsOfflineAsync()
        {
            SerializableResult<List<MaturedBlockDepositsModel>> result = await this.client.GetMaturedBlockDepositsAsync(100);
            Assert.Null(result);
        }
    }
}