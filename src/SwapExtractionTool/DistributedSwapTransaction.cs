﻿namespace SwapExtractionTool
{
    public sealed class DistributedSwapTransaction
    {
        public int BlockHeight { get; set; }
        public string XlcAddress { get; set; }
        public decimal SenderAmount { get; set; }
        public string TransactionHash { get; set; }
        public bool TransactionBuilt { get; set; }
        public bool TransactionSent { get; set; }
        public string TransactionSentHash { get; set; }

        public DistributedSwapTransaction() { }

        public DistributedSwapTransaction(SwapTransaction swapTransaction)
        {
            this.BlockHeight = swapTransaction.BlockHeight;
            this.XlcAddress = swapTransaction.XlcAddress;
            this.SenderAmount = swapTransaction.SenderAmount;
            this.TransactionHash = swapTransaction.TransactionHash;
        }
    }
}