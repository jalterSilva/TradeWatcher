namespace TradeWatcher.Model
{
    public class YahooChartResponse
    {
        public Chart? Chart { get; set; }
    }

    public class Chart
    {
        public List<Result>? Result { get; set; }
        public object? Error { get; set; }
    }

    public class Result
    {
        public Meta? Meta { get; set; }
        public List<long>? Timestamp { get; set; }
        public Indicators? Indicators { get; set; }
    }

    public class Meta
    {
        public string? Currency { get; set; }
        public string? Symbol { get; set; }
        public string? ExchangeName { get; set; }
        public string? FullExchangeName { get; set; }
        public string? InstrumentType { get; set; }
    }

    public class Indicators
    {
        public List<Quote>? Quote { get; set; }
        public List<AdjCloseWrapper>? Adjclose { get; set; }
    }

    public class AdjCloseWrapper
    {
        public List<decimal>? Adjclose { get; set; }
    }

    public class Quote
    {
        public List<decimal>? Open { get; set; }
        public List<decimal>? High { get; set; }
        public List<decimal>? Low { get; set; }
        public List<decimal>? Close { get; set; }
        public List<long>? Volume { get; set; }
    }

}
