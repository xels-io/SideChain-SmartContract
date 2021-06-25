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
    public class CCMain : PoANetwork
    {
        public CCMain()
        {
            this.Name = "CCMain";
            this.NetworkType = NetworkType.Mainnet;
            this.CoinTicker = "XCC";
            this.Magic = 0x522357AC;
            this.DefaultPort = 27770;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 16175;
            this.DefaultAPIPort = 37223;
            this.DefaultSignalRPort = 38823;
            this.MaxTipAge = 768; // 20% of the fastest time it takes for one MaxReorgLength of blocks to be mined.
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = CCNetwork.NetworkRootFolderName;
            this.DefaultConfigFilename = CCNetwork.NetworkDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.DefaultBanTimeSeconds = 1920; // 240 (MaxReorg) * 16 (TargetSpacing) / 2 = 32 Minutes

            this.CCRewardDummyAddress = "CPqxvnzfXngDi75xBJKqi4e6YrFsinrJka";

            var consensusFactory = new SmartContractCollateralPoAConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1624274057;
            this.GenesisNonce = 3332089;
            this.GenesisBits = new Target(new uint256("00000fffff000000000000000000000000000000000000000000000000000000"));
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            string coinbaseText = "There is hope yet! We all need to work together. WE GOT THIS!!!";
            Block genesisBlock = CCNetwork.CreateGenesis(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, coinbaseText);

            this.Genesis = genesisBlock;

            // Configure federation public keys used to sign blocks.
            // Keep in mind that order in which keys are added to this list is important
            // and should be the same for all nodes operating on this network.
            var genesisFederationMembers = new List<IFederationMember>()
            {
                new CollateralFederationMember(new PubKey("033762e3baa6628ba1e523e0d3a4b0112f3704467aba0f0fd5788ddf308cd23a43"), true, new Money(7000000_00000000),"XME93DJogF1HH9iSvo1127oKRaHBxEoQfS"),
                new CollateralFederationMember(new PubKey("02b7b1b8802a5155dbf7ba0fdfca028e995c77da179972a8cf1e99d93462f91d01"), true, new Money(7000000_00000000),"XSy63BJycFb4QP1qDiU13hxKD5H3U2yhYJ"),
                new CollateralFederationMember(new PubKey("02e2a8ee8197604a86131b54e18273f9ccb43f58b9de8c4a611cf27c5e715aea2e"), true, new Money(7000000_00000000),"XYMzq6E3MHfgcWQi1DfjNt4YQmEpKPeA1B"),
                new CollateralFederationMember(new PubKey("03e54650fb2242613ce0b508df0650c3d7cbcb4b82c748c7b69157927c47b19642"), true, new Money(7000000_00000000),"CK7j3zkc1TbZVug289eq7UpKBy7dRWxox2"),
                new CollateralFederationMember(new PubKey("0394788094df4ddde7eb3eacdd61245eef3d03b103f566406aa43cb18c36a1e64b"), true, new Money(7000000_00000000),"CZ9gAuEHX9oQiCV9EfwDinRQ96UQfB1CDq"),
                new CollateralFederationMember(new PubKey("02fd05bd7398e3e36d93c31a91b603418f9e4e1b251490b790eabb9cc302927be8"), true, new Money(7000000_00000000),"Cev8YBzpFZFmfMQLeKSNYgAa57CmyqwhUT"),
                new CollateralFederationMember(new PubKey("03caac79a12ffd58595d5eb1a3cda5cd5cce5fd82b595181d6bbf70514e48fcbff"), true, new Money(7000000_00000000),"Cev8YBzpFZFmfMQLeKSNYgAa57CmyqwhUT"),
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
                EnforceMinProtocolVersionAtBlockHeight = 384675, // setting the value to zero makes the functionality inactive
                EnforcedMinProtocolVersion = ProtocolVersion.CC_VERSION, // minimum protocol version which will be enforced at block height defined in EnforceMinProtocolVersionAtBlockHeight
                FederationMemberActivationTime = 1605862800, // Friday, November 20, 2020 9:00:00 AM
                VotingManagerV2ActivationHeight = 1_683_000 // Tuesday, 12 January 2021 9:00:00 AM (Estimated)
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
                bip34Hash: new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 240, // Heuristic. Roughly 2 * mining members
                defaultAssumeValid: new uint256("0xbfd4a96a6c5250f18bf7c586761256fa5f8753ffa10b24160f0648a452823a95"), // 1400000
                maxMoney: Money.Coins(100_000_000),
                coinbaseMaturity: 1,
                premineHeight: 2,
                premineReward: Money.Coins(100_000_000),
                proofOfWorkReward: Money.Coins(0),
                powTargetTimespan: TimeSpan.FromDays(14), // two weeks
                targetSpacing: TimeSpan.FromSeconds(16),
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
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 88 }; // c
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
                { 50000, new CheckpointInfo(new uint256("0xf3ed37db1c56751fdf9f45902696dd034444a697cd8c106a08f4c60cd2de9d77")) },
                { 100000, new CheckpointInfo(new uint256("0x1400cb20800d54cd7fff5fea90133a1a8ca44e7043268cd0c7efdd7f8186b2d0")) },
                { 150000, new CheckpointInfo(new uint256("0x505d22805f0fc4ea057edad778e7334412526a7c1b017b179be5d274c8d42914")) },
                { 200000, new CheckpointInfo(new uint256("0x5569221c600e42b0467c92bd932046c12198eee5c50ac98eadff7d3159f55b75")) },
                { 250000, new CheckpointInfo(new uint256("0x1a0d5f43335eff00e8a3b5dc09e4f6849b571b6870eb58364cf86623222922d7")) },
                { 300000, new CheckpointInfo(new uint256("0x3b1c3704e0cb79e7fff46ab7e9feacbfa9e2e95ab90b273d99520dbd42cc34b6")) },
                { 350000, new CheckpointInfo(new uint256("0xcb420b8ef20e1da9eb63b6847005b17928b4bad6c2920eebc964ecf21c50ce5a")) },
                { 400000, new CheckpointInfo(new uint256("0xa501a5c69dfce78e39bf0c25d2c1eafa9fd7a9f32ee06b419d3a3c0a6ac29d8b")) },
                { 450000, new CheckpointInfo(new uint256("0xc3ae6119d23294ac51c05f9c761da5271711b1945592cb83cc1bcc1b908780c7")) },
                { 500000, new CheckpointInfo(new uint256("0x810cc011d6d5158aaefcc38550a31b4118fae1bb18ea7894f81a2edc81126d5f")) },
                { 550000, new CheckpointInfo(new uint256("0x3a6b0a58deb1997879d35fc6e017123594c00eafb3ac45d8c31a5dbf68c2bccc")) },
                { 600000, new CheckpointInfo(new uint256("0xc79bf7066ec9243a335fcd2a43380a47a5b9dccdeaee3f67ab5503cef0cd1626")) },
                { 700000, new CheckpointInfo(new uint256("0xe777ae5e283564a994cbcf88315a594854c12d626e6908fb27e3d0cd7d04fcc7")) },
                { 800000, new CheckpointInfo(new uint256("0xe8b2b9b4e342b0ff9a0b1b967b0f2b7481fe420c5922322d1b77cfae66471fa1")) },
                { 900000, new CheckpointInfo(new uint256("0x30599fbbce4404ebaff9f8d0ea7071c684f124439f1f4e9fabec0debad6c7a06")) },
                { 1_000_000, new CheckpointInfo(new uint256("0x547faf99acb45e2195ea5fbb6873562c44a7696f6571e8a309d6c9f509be064a")) },
                { 1_100_000, new CheckpointInfo(new uint256("0x7abc2882bcb5e9723ba71ff4155ed3c4006ee655e9f52f8787bcae31b4c796a8")) },
                { 1_200_000, new CheckpointInfo(new uint256("0x8411b830270cc9d6c2e28de1c2e8025c57a5673835f63e30708967adfee5a92c")) },
                { 1_300_000, new CheckpointInfo(new uint256("0x512c19a8245316b4d3b13513c7901f41842846f539f668ca4ac349daaab6dc20")) },
                { 1_400_000, new CheckpointInfo(new uint256("0xbfd4a96a6c5250f18bf7c586761256fa5f8753ffa10b24160f0648a452823a95")) },
                { 1_500_000, new CheckpointInfo(new uint256("0x2a1602877a5231997654bae975223762ee636be2f371cb444b2d3fb564e6989e")) },
                { 1_750_000, new CheckpointInfo(new uint256("0x58c96a878efeeffea1b1924b61eed627687900e01588ffaa2f4a161973f01abf")) },
                { 1_850_000, new CheckpointInfo(new uint256("0x6e2590bd9a8eaab25b236c0c9ac314abec70b18aa053b96c9257f2356dec8314")) },
                { 2_150_000, new CheckpointInfo(new uint256("0x4c65f29b5098479cab275afd77d302ebe5ed8d8ef33e02ae54bf185865763f18")) },
            };

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("CCmain1.Xelsnetwork.com", "CCmain1.Xelsnetwork.com")
            };

            this.SeedNodes = new List<NetworkAddress>
            {
                new NetworkAddress(IPAddress.Parse("213.125.242.234"), 16179),
                new NetworkAddress(IPAddress.Parse("45.58.55.21"), 16179),
                new NetworkAddress(IPAddress.Parse("86.106.181.141"), 16179),
                new NetworkAddress(IPAddress.Parse("51.195.136.221"), 16179)
            };

            this.StandardScriptsRegistry = new PoAStandardScriptsRegistry();

            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * this.Consensus.TargetSpacing.TotalSeconds / 2);
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("000005769503496300ec879afd7543dc9f86d3b3d679950b2b83e2f49f525856"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("1669a55d45b642af0ce82c5884cf5b8d8efd5bdcb9a450c95f442b9bd1ff65ea"));

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
