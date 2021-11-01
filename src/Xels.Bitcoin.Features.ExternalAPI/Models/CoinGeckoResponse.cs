namespace Xels.Bitcoin.Features.ExternalApi.Models
{
    public class CoinGeckoPriceData
    {
        public decimal usd { get; set; }
    }

    public class CoinGeckoResponse
    {
        public CoinGeckoPriceData xels { get; set; }

        public CoinGeckoPriceData ethereum { get; set; }
    }
}
