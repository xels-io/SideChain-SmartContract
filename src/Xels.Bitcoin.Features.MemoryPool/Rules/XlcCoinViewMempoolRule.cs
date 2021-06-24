using Microsoft.Extensions.Logging;
using NBitcoin;
using Xels.Bitcoin.Features.Consensus.Rules.CommonRules;
using Xels.Bitcoin.Features.MemoryPool.Interfaces;
using Xels.Bitcoin.Utilities;

namespace Xels.Bitcoin.Features.MemoryPool.Rules
{
    /// <summary>
    /// Validates the transaction with the coin view.
    /// Checks if already in coin view, and missing and unavailable inputs.
    /// </summary>
    public class XlcCoinViewMempoolRule : CheckCoinViewMempoolRule
    {
        public XlcCoinViewMempoolRule(Network network,
            ITxMempool mempool,
            MempoolSettings mempoolSettings,
            ChainIndexer chainIndexer,
            ILoggerFactory loggerFactory) : base(network, mempool, mempoolSettings, chainIndexer, loggerFactory)
        {
        }

        /// <remarks>Also see <see cref="XlcCoinviewRule"/></remarks>>
        public override void CheckTransaction(MempoolValidationContext context)
        {
            base.CheckTransaction(context);

            foreach (TxIn txin in context.Transaction.Inputs)
            {
                // We expect that by this point the base rule will have checked for missing inputs.
                UnspentOutput unspentOutput = context.View.Set.AccessCoins(txin.PrevOut);
                if (unspentOutput?.Coins == null)
                {
                    context.State.MissingInputs = true;
                    this.logger.LogTrace("(-)[FAIL_MISSING_INPUTS_ACCESS_COINS]");
                    context.State.Fail(MempoolErrors.MissingOrSpentInputs).Throw();
                }

                if (unspentOutput.Coins.TxOut.ScriptPubKey == XlcCoinstakeRule.CCRewardScript)
                {
                    this.logger.LogDebug("Reward distribution transaction seen in mempool, paying to '{0}'.", unspentOutput.Coins.TxOut.ScriptPubKey);

                    foreach (TxOut output in context.Transaction.Outputs)
                    {
                        if (output.ScriptPubKey.IsUnspendable)
                        {
                            if (output.Value != 0)
                            {
                                this.logger.LogTrace("(-)[INVALID_REWARD_OP_RETURN_SPEND]");
                                context.State.Fail(new MempoolError(MempoolErrors.RejectInvalid, "bad-CC-reward-tx-opreturn-not-zero"), "CC reward transaction invalid, op_return value is not 0.").Throw();
                            }

                            continue;
                        }

                        // Every other (spendable) output must go to the multisig
                        if (output.ScriptPubKey != this.network.Federations.GetOnlyFederation().MultisigScript.PaymentScript)
                        {
                            this.logger.LogTrace("(-)[INVALID_REWARD_SPEND_DESTINATION]");
                            context.State.Fail(new MempoolError(MempoolErrors.RejectInvalid, "bad-CC-reward-tx-reward-dest-invalid"), "CC reward transaction invalid, reward destination invalid.").Throw();
                        }
                    }
                }
            }
        }
    }
}
