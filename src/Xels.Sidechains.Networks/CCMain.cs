using System;
using System.Collections.Generic;
using System.Net;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Xels.Bitcoin.Features.Consensus.Rules.CommonRules;
using Xels.Bitcoin.Features.MemoryPool.Rules;
using Xels.Bitcoin.Features.PoA;
using Xels.Bitcoin.Features.PoA.BasePoAFeatureConsensusRules;
using Xels.Bitcoin.Features.PoA.Policies;
using Xels.Bitcoin.Features.PoA.Voting.ConsensusRules;
using Xels.Bitcoin.Features.SmartContracts.MempoolRules;
using Xels.Bitcoin.Features.SmartContracts.PoA;
using Xels.Bitcoin.Features.SmartContracts.PoA.Rules;
using Xels.Bitcoin.Features.SmartContracts.Rules;

namespace Xels.Sidechains.Networks
{
    /// <summary>
    /// <see cref="PoANetwork"/>.
    /// </summary>
    public class CcMain : PoANetwork
    {
        public CcMain()
        {
            this.Name = "CcMain";
            this.NetworkType = NetworkType.Mainnet;
            this.CoinTicker = "XCC";
            this.Magic = 0x522357AC;
            this.DefaultPort = 27771;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 16175;
            this.DefaultAPIPort = 37223;
            this.DefaultSignalRPort = 38823;
            this.MaxTipAge = 768; // 20% of the fastest time it takes for one MaxReorgLength of blocks to be mined.
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = CcNetwork.NetworkRootFolderName;
            this.DefaultConfigFilename = CcNetwork.NetworkDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.DefaultBanTimeSeconds = 1920; // 240 (MaxReorg) * 16 (TargetSpacing) / 2 = 32 Minutes

            this.CcRewardDummyAddress = "CKe36GSqPx3EasYY9FevtLSnyx5nryojaN";

            this.ConversionTransactionFeeDistributionDummyAddress = "CKe36GSqPx3EasYY9FevtLSnyx5nryojaN";

            var consensusFactory = new SmartContractCollateralPoAConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1624274057;
            this.GenesisNonce = 3332089;
            this.GenesisBits = new Target(new uint256("00000fffff000000000000000000000000000000000000000000000000000000"));
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            string coinbaseText = "There is hope yet! We all need to work together. WE GOT THIS!!!";
            Block genesisBlock = CcNetwork.CreateGenesis(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, coinbaseText);

            this.Genesis = genesisBlock;

            // Configure federation public keys used to sign blocks.
            // Keep in mind that order in which keys are added to this list is important
            // and should be the same for all nodes operating on this network.
            var genesisFederationMembers = new List<IFederationMember>()
            {
                new CollateralFederationMember(new PubKey("033762e3baa6628ba1e523e0d3a4b0112f3704467aba0f0fd5788ddf308cd23a43"), true,  new Money(50000_00000000),"XPddcvptCSW1XQeGJzmNruXYDmt2Yo649s"),
                new CollateralFederationMember(new PubKey("02b7b1b8802a5155dbf7ba0fdfca028e995c77da179972a8cf1e99d93462f91d01"), true,  new Money(50000_00000000),"XG2752izyfuui6Cys7rqATx3JS5zNgL3wQ"),
                new CollateralFederationMember(new PubKey("02e2a8ee8197604a86131b54e18273f9ccb43f58b9de8c4a611cf27c5e715aea2e"), true,  new Money(50000_00000000),"XHeUcZtFp9TLiK3nGbi7QcCN2PAjPxUSHX"),
                new CollateralFederationMember(new PubKey("03e54650fb2242613ce0b508df0650c3d7cbcb4b82c748c7b69157927c47b19642"), true,  new Money(0), null),//"CcwX6LmbL1ZUxgbdVz1pDe25SmmNtLnnTr"),
                new CollateralFederationMember(new PubKey("0394788094df4ddde7eb3eacdd61245eef3d03b103f566406aa43cb18c36a1e64b"), true,  new Money(0), null),//"CHwG8nrqs3yDhNwBnzcdtSpuFNWHfUkhoG"),
                new CollateralFederationMember(new PubKey("02fd05bd7398e3e36d93c31a91b603418f9e4e1b251490b790eabb9cc302927be8"), true,  new Money(0), null),//"CWvxWKHyzBzEEpcX2J1CCXZGzQKps2ux9j"),
                new CollateralFederationMember(new PubKey("03caac79a12ffd58595d5eb1a3cda5cd5cce5fd82b595181d6bbf70514e48fcbff"), false, new Money(50),"XGytWoLG4mcoMNXN387D3RJtUvsXwo1aNX") //"XVGhRi1wSk4idA9NCZ9eXtrrLWti1iSPRP"),
            };

            this.Federations = new Federations();
            var xlcFederationTransactionSigningKeys = new List<PubKey>()
            {
                new PubKey("02b5ae6ae2997c33ad728d5a65a91c212789c3bd61015db5675d487020ba5bfe0d"),
                new PubKey("021335d02d35b21527b1db28d1bd37cb4376700aa77709eb7106ebb56b221b9395"),
                new PubKey("037e7aa5e9dd41de242a842530e893c256bd76234708d44081e5baabd39d411cc7")
            };

            // Register the new set of federation members.
            this.Federations.RegisterFederation(new Federation(xlcFederationTransactionSigningKeys));

            // The height at which the following list of members apply.
            //this.MultisigMinersApplicabilityHeight = 1413998;

            // Set the list of Xlc Era mining keys.
            this.XlcMiningMultisigMembers = new List<PubKey>()
            {
                new PubKey("033762e3baa6628ba1e523e0d3a4b0112f3704467aba0f0fd5788ddf308cd23a43"),
                new PubKey("02b7b1b8802a5155dbf7ba0fdfca028e995c77da179972a8cf1e99d93462f91d01"),
                new PubKey("02e2a8ee8197604a86131b54e18273f9ccb43f58b9de8c4a611cf27c5e715aea2e"),
            };

            var consensusOptions = new PoAConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 150_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                genesisFederationMembers: genesisFederationMembers,
                targetSpacingSeconds: 16,
                votingEnabled: true,
                autoKickIdleMembers: true,
                federationMemberMaxIdleTimeSeconds: 60 * 60 * 24 * 2 // 2 days
            )
            {
                EnforceMinProtocolVersionAtBlockHeight = 0, // setting the value to zero makes the functionality inactive
                EnforcedMinProtocolVersion = ProtocolVersion.CC_VERSION, // minimum protocol version which will be enforced at block height defined in EnforceMinProtocolVersionAtBlockHeight
                FederationMemberActivationTime = 1605862800, // Friday, November 20, 2020 9:00:00 AM
                InterFluxV2MainChainActivationHeight = 1,
                VotingManagerV2ActivationHeight = 1, // Tuesday, 12 January 2021 9:00:00 AM (Estimated)
                Release1100ActivationHeight = 1,
                PollExpiryBlocks = 50_000 // Roughly 9 days
            };

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new NoBIP9Deployments();

            this.Consensus = new Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 401,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: null,  //new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 240, // Heuristic. Roughly 2 * mining members
                defaultAssumeValid: null,  //new uint256("0x0000005a39d58d384efabc1a9c79cd6e8c63894c77d4219526481cb49582ff29"), // 1400000
                maxMoney: Money.Coins(100_000_000),
                coinbaseMaturity: 1,
                premineHeight: 2,
                premineReward: Money.Coins(100_000_000),
                proofOfWorkReward: Money.Coins(40),
                powTargetTimespan: TimeSpan.FromDays(14), // two weeks
                targetSpacing: TimeSpan.FromSeconds(160),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: true,
                powLimit: null,
                minimumChainWork: null,
                isProofOfStake: false,
                lastPowBlock: 0,
                proofOfStakeLimit: null,
                proofOfStakeLimitV2: null,
                proofOfStakeReward: Money.Zero
            );

            // Same as current smart contracts test networks to keep tests working
            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 28 }; // C
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 28 }; // c - 88
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (239) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x35), (0x87), (0xCF) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x35), (0x83), (0x94) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2b };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 115 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            Bech32Encoder encoder = Encoders.Bech32("tb");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.Checkpoints = new Dictionary<int, CheckpointInfo>()
            {

            };

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("api.xels.io","api.xels.io"),
                new DNSSeedData("mainnet.xels.io","mainnet.xels.io")
            };

            this.SeedNodes = new List<NetworkAddress>
            {
                new NetworkAddress(IPAddress.Parse("52.68.239.4"), this.DefaultPort ), // public node with DNS Server Enabled
                new NetworkAddress(IPAddress.Parse("54.64.43.45"), this.DefaultPort ), // public node with DNS Server Enabled
                new NetworkAddress(IPAddress.Parse("54.238.248.117"), this.DefaultPort), // public node
                new NetworkAddress(IPAddress.Parse("13.114.52.87"), this.DefaultPort), // public node
                new NetworkAddress(IPAddress.Parse("52.192.229.45"), this.DefaultPort), // public node
                new NetworkAddress(IPAddress.Parse("52.199.121.139"), this.DefaultPort ) // public node
            };

            //this.CollateralCommitmentActivationHeight = 25810;

            this.StandardScriptsRegistry = new PoAStandardScriptsRegistry();

            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * this.Consensus.TargetSpacing.TotalSeconds / 2);
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0000005a39d58d384efabc1a9c79cd6e8c63894c77d4219526481cb49582ff29"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("64637abf3dd01134a4c7038916396f5cabe26c64c14a41ddd633389a7f4c28c2"));

            this.RegisterRules(this.Consensus);
            this.RegisterMempoolRules(this.Consensus);
        }

        // This should be abstract or virtual
        protected override void RegisterRules(IConsensus consensus)
        {
            // IHeaderValidationConsensusRule -----------------------
            consensus.ConsensusRules
                .Register<HeaderTimeChecksPoARule>()
                .Register<XelsHeaderVersionRule>()
                .Register<PoAHeaderDifficultyRule>();
            // ------------------------------------------------------

            // IIntegrityValidationConsensusRule
            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PoAIntegritySignatureRule>();
            // ------------------------------------------------------

            // IPartialValidationConsensusRule
            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()

                // Rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<BlockSizeRule>()

                // Rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckSigOpsRule>()

                .Register<PoAVotingCoinbaseOutputFormatRule>()
                .Register<AllowedScriptTypeRule>()
                .Register<ContractTransactionPartialValidationRule>();
            // ------------------------------------------------------

            // IFullValidationConsensusRule
            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                // Rules that require the store to be loaded (coinview)
                .Register<PoAHeaderSignatureRule>()
                .Register<LoadCoinviewRule>()
                .Register<TransactionDuplicationActivationRule>() // implements BIP30

                // Smart contract specific
                .Register<ContractTransactionFullValidationRule>()
                .Register<TxOutSmartContractExecRule>()
                .Register<OpSpendRule>()
                .Register<CanGetSenderRule>()
                .Register<P2PKHNotContractRule>()
                .Register<SmartContractPoACoinviewRule>()
                .Register<SaveCoinviewRule>();
            // ------------------------------------------------------
        }

        protected override void RegisterMempoolRules(IConsensus consensus)
        {
            consensus.MempoolRules = new List<Type>()
            {
                typeof(OpSpendMempoolRule),
                typeof(TxOutSmartContractExecMempoolRule),
                typeof(AllowedScriptTypeMempoolRule),
                typeof(P2PKHNotContractMempoolRule),

                // The non- smart contract mempool rules
                typeof(CheckConflictsMempoolRule),
                typeof(CheckCoinViewMempoolRule),
                typeof(CreateMempoolEntryMempoolRule),
                typeof(CheckSigOpsMempoolRule),
                typeof(CheckFeeMempoolRule),

                // The smart contract mempool needs to do more fee checks than its counterpart, so include extra rules.
                // These rules occur directly after the fee check rule in the non- smart contract mempool.
                typeof(SmartContractFormatLogicMempoolRule),
                typeof(CanGetSenderMempoolRule),
                typeof(AllowedCodeHashLogicMempoolRule), // PoA-specific
                typeof(CheckMinGasLimitSmartContractMempoolRule),

                // Remaining non-SC rules.
                typeof(CheckRateLimitMempoolRule),
                typeof(CheckAncestorsMempoolRule),
                typeof(CheckReplacementMempoolRule),
                typeof(CheckAllInputsMempoolRule)
            };
        }
    }
}
