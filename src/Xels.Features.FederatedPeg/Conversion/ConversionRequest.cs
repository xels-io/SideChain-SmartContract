using NBitcoin;

namespace Xels.Features.FederatedPeg.Conversion
{
    public enum ConversionRequestType
    {
        Mint,
        Burn
    }

    public enum ConversionRequestStatus
    {
        Unprocessed,
        Submitted, // Unused, need to keep it due to previous state ordering in database
        Processed,

        // States particular to Mint transactions
        OriginatorNotSubmitted,
        OriginatorSubmitted,
        VoteFinalised,
        NotOriginator
    }

    /// <summary>Chains supported by InterFlux integration.</summary>
    public enum DestinationChain
    {
        XLC = 0, // Xels
        ETH, // Ethereum
        BNB, // Binance

        ETC, // Ethereum classic
        AVAX, // Avalanche
        ADA, // Cardano
    }

    /// <summary>Request to mint or burn wXLC.</summary>
    /// <remarks>
    /// When wXLC coins are minted and sent to <see cref="DestinationAddress"/> on ETH chain same amount of XLC coins should be received by the multisig address.
    /// When wXLC coins are burned on ETH chain same amount of XLC coins should be sent to <see cref="DestinationAddress"/>.
    /// </remarks>
    public class ConversionRequest : IBitcoinSerializable
    {
        /// <summary>
        /// The unique identifier for this particular conversion request.
        /// It gets selected by the request creator.
        /// The request ID is typically the initiating transaction ID.
        /// </summary>
        public string RequestId { get { return this.requestId; } set { this.requestId = value; } }

        /// <summary>
        /// The type of the conversion request, mint or burn.
        /// </summary>
        public ConversionRequestType RequestType { get { return (ConversionRequestType)this.requestType; } set { this.requestType = (int)value; } }

        /// <summary>
        /// The status of the request, from unprocessed to processed.
        /// </summary>
        public ConversionRequestStatus RequestStatus { get { return (ConversionRequestStatus)this.requestStatus; } set { this.requestStatus = (int)value; } }

        /// <summary>
        /// For a mint request this is needed to coordinate which multisig member is considered the transaction originator on the wallet contract.
        /// A burn request needs to be scheduled for a future block on the main chain so that the conversion can be cleanly inserted into the sequence
        /// of transfers.
        /// </summary>
        public int BlockHeight { get { return this.blockHeight; } set { this.blockHeight = value; } }

        /// <summary>
        /// Either the Ethereum address to send the minted funds to, or the XLC address to send unwrapped wXLC funds to.
        /// </summary>
        public string DestinationAddress { get { return this.destinationAddress; } set { this.destinationAddress = value; } }

        /// <summary>Chain on which XLC minting or burning should occur.</summary>
        public DestinationChain DestinationChain { get { return (DestinationChain)this.destinationChain; } set { this.destinationChain = (int)value; } }

        /// <summary>
        /// Amount of the conversion, this is always denominated in satoshi. This needs to be converted to wei for submitting mint transactions.
        /// Burn transactions are already denominated in wei on the Ethereum chain and thus need to be converted back into satoshi when the
        /// conversion request is created. Conversions are currently processed 1 ether : 1 XLC.
        /// </summary>
        public ulong Amount { get { return this.amount; } set { this.amount = value; } }

        /// <summary>
        /// Indicates whether or not this request has been processed by the interop poller.
        /// </summary>
        public bool Processed { get { return this.processed; } set { this.processed = value; } }

        private string requestId;

        private int requestType;

        private int requestStatus;

        private int blockHeight;

        private string destinationAddress;

        private int destinationChain;

        private ulong amount;

        private bool processed;

        public void ReadWrite(BitcoinStream s)
        {
            s.ReadWrite(ref this.requestId);
            s.ReadWrite(ref this.requestType);
            s.ReadWrite(ref this.requestStatus);
            s.ReadWrite(ref this.blockHeight);
            s.ReadWrite(ref this.destinationAddress);
            s.ReadWrite(ref this.destinationChain);
            s.ReadWrite(ref this.amount);
            s.ReadWrite(ref this.processed);
        }
    }
}
