namespace TradeWatcher.Model
{
    public class Candle
    {
        public DateTimeOffset Date { get; set; }
        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
    }
}
