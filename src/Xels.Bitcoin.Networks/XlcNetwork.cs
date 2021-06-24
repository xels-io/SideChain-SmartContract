﻿using System;
using System.Collections.Generic;
using NBitcoin;
using NBitcoin.DataEncoders;
using Xels.Bitcoin.Features.Consensus.Rules.CommonRules;
using Xels.Bitcoin.Features.Consensus.Rules.ProvenHeaderRules;
using Xels.Bitcoin.Features.MemoryPool.Rules;

namespace Xels.Bitcoin.Networks
{
    public static class XlcNetwork
    {
        /// <summary> Xels maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int XelsMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Xels default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int XelsDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Xels blockchains (XelsMain, XelsTest, XelsRegTest). </summary>
        public const string XlcRootFolderName = "xlc";

        /// <summary> The default name used for the Xlc configuration file. </summary>
        public const string XlcDefaultConfigFilename = "xlc.conf";

        public static void RegisterRules(IConsensus consensus)
        {
            consensus.ConsensusRules
                .Register<HeaderTimeChecksRule>()
                .Register<HeaderTimeChecksPosRule>()
                .Register<PosFutureDriftRule>()
                .Register<CheckDifficultyPosRule>()
                .Register<XelsHeaderVersionRule>()
                .Register<ProvenHeaderSizeRule>()
                .Register<ProvenHeaderCoinstakeRule>();

            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PosBlockSignatureRepresentationRule>()
                .Register<PosBlockSignatureRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()
                .Register<PosTimeMaskRule>()

                // rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<WitnessCommitmentsRule>()
                .Register<BlockSizeRule>()

                // rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckPosTransactionRule>()
                .Register<CheckSigOpsRule>()
                .Register<XlcCoinstakeRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                .Register<CheckDifficultyHybridRule>()

                // rules that require the store to be loaded (coinview)
                .Register<LoadCoinviewRule>()
                .Register<TransactionDuplicationActivationRule>()
                .Register<XlcCoinviewRule>() // implements BIP68, MaxSigOps and BlockReward calculation
                                             // Place the PosColdStakingRule after the PosCoinviewRule to ensure that all input scripts have been evaluated
                                             // and that the "IsColdCoinStake" flag would have been set by the OP_CHECKCOLDSTAKEVERIFY opcode if applicable.
                .Register<XlcColdStakingRule>()
                .Register<SaveCoinviewRule>();
        }

        public static void RegisterMempoolRules(IConsensus consensus)
        {
            consensus.MempoolRules = new List<Type>()
            {
                typeof(CheckConflictsMempoolRule),
                typeof(XlcCoinViewMempoolRule),
                typeof(CreateMempoolEntryMempoolRule),
                typeof(CheckSigOpsMempoolRule),
                typeof(XlcTransactionFeeMempoolRule),
                typeof(CheckRateLimitMempoolRule),
                typeof(CheckAncestorsMempoolRule),
                typeof(CheckReplacementMempoolRule),
                typeof(CheckAllInputsMempoolRule),
                typeof(CheckTxOutDustRule)
            };
        }

        public static Block CreateGenesisBlock(ConsensusFactory consensusFactory, uint time, uint nonce, uint bits, int version, Money genesisReward, string genesisText)
        {
            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(genesisText)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(time);
            genesis.Header.Bits = bits;
            genesis.Header.Nonce = nonce;
            genesis.Header.Version = version;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();

            /*
            Procedure for creating a new genesis block:
            1. Create the template block as above in the CreateXlcGenesisBlock method

            3. Iterate over the nonce until the proof-of-work is valid
            */

            //while (!genesis.CheckProofOfWork())
            //{
            //   genesis.Header.Nonce++;
            //   if (genesis.Header.Nonce == 0)
            //       genesis.Header.Time++;
            //}

            /*
            4. This will mean the block header hash is under the target
            5. Retrieve the Nonce and Time values from the resulting block header and insert them into the network definition
            */

            return genesis;

        }

        public static readonly Dictionary<NetworkType, Func<Network>> MainChainNetworks = new Dictionary<NetworkType, Func<Network>>
        {
            { NetworkType.Mainnet, Networks.Xlc.Mainnet },
            { NetworkType.Testnet, Networks.Xlc.Testnet },
            { NetworkType.Regtest, Networks.Xlc.Regtest }
        };
    }
}
