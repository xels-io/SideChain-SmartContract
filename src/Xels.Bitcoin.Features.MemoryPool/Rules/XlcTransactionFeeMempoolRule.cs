﻿using System.Linq;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Xels.Bitcoin.Features.Consensus.Rules.CommonRules;
using Xels.Bitcoin.Features.MemoryPool.Interfaces;

namespace Xels.Bitcoin.Features.MemoryPool.Rules
{
    /// <summary>
    /// Checks if the transaction is paying rewards to the federation multisig.
    /// If so, fee checks get skipped (as these reward claiming transactions have to be constructed without fees).
    /// Transactions that do not conform to this structure must be kept out of the mempool, as trying to include them
    /// in blocks for mining/staking would fail consensus validation.
    /// </summary>
    public class XlcTransactionFeeMempoolRule : CheckFeeMempoolRule
    {
        public XlcTransactionFeeMempoolRule(Network network,
            ITxMempool mempool,
            MempoolSettings mempoolSettings,
            ChainIndexer chainIndexer,
            ILoggerFactory loggerFactory) : base(network, mempool, mempoolSettings, chainIndexer, loggerFactory)
        {
        }

        public override void CheckTransaction(MempoolValidationContext context)
        {
            // We expect a reward claim transaction to have at least 2 outputs.
            bool federationPayment = !(context.Transaction.Outputs.Count < 2);

            // The OP_RETURN output that marks the transaction as cross-chain (and in particular a reward claiming transaction) must be present.
            if (context.Transaction.Outputs.All(o => o.ScriptPubKey != XlcCoinstakeRule.CCTransactionTag(this.network.CCRewardDummyAddress)))
            {
                federationPayment = false;
            }

            // At least one other output must be paying to the multisig.
            if (context.Transaction.Outputs.All(o => o.ScriptPubKey != this.network.Federations.GetOnlyFederation().MultisigScript.PaymentScript))
            {
                federationPayment = false;
            }

            // There must be no other spendable scriptPubKeys.
            if (context.Transaction.Outputs.Any(o => o.ScriptPubKey != this.network.Federations.GetOnlyFederation().MultisigScript.PaymentScript && !o.ScriptPubKey.IsUnspendable))
            {
                federationPayment = false;
            }

            // We need to bypass the fee checking logic for correctly-formed transactions that pay CC rewards to the federation.
            if (federationPayment)
                return;

            base.CheckTransaction(context);
        }
    }
}
