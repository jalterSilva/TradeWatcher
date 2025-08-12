namespace InsiderTrade.Options
{
    public sealed class InsiderTradeOptions
    {
        public int IntervalMinutes { get; set; }
        public int CandleMinutes { get; set; }
        public string[] Underlyings { get; set; } = [];
        public SeriesOptions Series { get; set; } = new();
        public SeverityOptions Severity { get; set; } = new();
    }

    public sealed class SeriesOptions
    {
        public string CurrentCall { get; set; } = string.Empty;
        public string CurrentPut { get; set; } = string.Empty;
        public string[] Calls { get; set; } = [];
        public string[] Puts { get; set; } = [];
    }

    public sealed class SeverityOptions
    {
        public long Star1Min { get; set; }
        public long Star2Min { get; set; }
        public long Star3Min { get; set; }
    }

    public sealed class OpLabOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = "v3";
        public string AccessToken { get; set; } = string.Empty; // virá do User Secrets
    }
}
