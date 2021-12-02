using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Xels.Bitcoin.Features.Consensus.Rules.CommonRules;
using Xels.Bitcoin.Features.MemoryPool.Interfaces;

namespace Xels.Bitcoin.Features.MemoryPool.Rules
{
    public class PchTransactionFeeMempoolRule : CheckFeeMempoolRule
    {
        public PchTransactionFeeMempoolRule(Network network,
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
            if (context.Transaction.Outputs.All(o => o.ScriptPubKey != PchCoinstakeRule.CcTransactionTag(this.network.CcRewardDummyAddress)))
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

            // We need to bypass the fee checking logic for correctly-formed transactions that pay Cc rewards to the federation.
            if (federationPayment)
                return;

            base.CheckTransaction(context);
        }
    }
}
