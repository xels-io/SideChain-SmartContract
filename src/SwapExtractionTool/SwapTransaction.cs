namespace SwapExtractionTool
{
    public sealed class SwapTransaction
    {
        public int BlockHeight { get; set; }
        public string XlcAddress { get; set; }
        public decimal SenderAmount { get; set; }
        public string TransactionHash { get; set; }
    }
}