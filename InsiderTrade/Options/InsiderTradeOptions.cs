namespace InsiderTrade.Options;

public sealed class InsiderTradeOptions
{
    public int IntervalMinutes { get; set; }
    public int CandleMinutes { get; set; }
    public List<string> Underlyings { get; set; } = new();

    public SeriesOptions Series { get; set; } = new();
    public SeverityOptions Severity { get; set; } = new();
}

public sealed class SeriesOptions
{
    public string CurrentCall { get; set; } = "";
    public string CurrentPut { get; set; } = "";
    public List<string> Calls { get; set; } = new();
    public List<string> Puts { get; set; } = new();
}

public sealed class SeverityOptions
{
    public long Star1Min { get; set; }
    public long Star2Min { get; set; }
    public long Star3Min { get; set; }
}
