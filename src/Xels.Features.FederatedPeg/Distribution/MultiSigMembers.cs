using System.Collections.Generic;
using NBitcoin;
using Xels.Bitcoin.Utilities;

namespace Xels.Features.FederatedPeg.Distribution
{
    public static class MultiSigMembers
    {
        public static bool IsContractOwner(Network network, PubKey pubKey)
        {
            if (network.IsTest())
                return InteropMultisigContractPubKeysTestNet.Contains(pubKey);
            else if (network.IsRegTest())
                return true;
            else
                return InteropMultisigContractPubKeysMainNet.Contains(pubKey);
        }

        /// <summary>
        /// This is the current set of multisig members that are participating in the multisig contract.
        /// </summary>
        /// <remarks>TODO: Refactor to make this list dynamic.</remarks>
        private static readonly List<PubKey> InteropMultisigContractPubKeysMainNet = new List<PubKey>()
        {
            new PubKey("033762e3baa6628ba1e523e0d3a4b0112f3704467aba0f0fd5788ddf308cd23a43"),
            new PubKey("02b7b1b8802a5155dbf7ba0fdfca028e995c77da179972a8cf1e99d93462f91d01"),
            new PubKey("02e2a8ee8197604a86131b54e18273f9ccb43f58b9de8c4a611cf27c5e715aea2e")
        };

        /// <summary>
        /// This is the current set of multisig members that are participating in the multisig contract.
        /// </summary>
        /// <remarks>TODO: Refactor to make this list dynamic.</remarks>
        private static readonly List<PubKey> InteropMultisigContractPubKeysTestNet = new List<PubKey>()
        {
            new PubKey("03cfc06ef56352038e1169deb3b4fa228356e2a54255cf77c271556d2e2607c28c"), // Cc 1
            new PubKey("02fc828e06041ae803ab5378b5ec4e0def3d4e331977a69e1b6ef694d67f5c9c13"), // Cc 3
            new PubKey("02fd4f3197c40d41f9f5478d55844f522744258ca4093b5119571de1a5df1bc653"), // Cc 4
        };
    }
}
