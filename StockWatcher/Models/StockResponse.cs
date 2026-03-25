using System.Text.Json.Serialization;

namespace StockWatcher.Models;

public sealed class StockResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("open")]
    public decimal? Open { get; set; }

    [JsonPropertyName("close")]
    public decimal? Close { get; set; }

    [JsonPropertyName("variation")]
    public decimal? Variation { get; set; }

    [JsonPropertyName("previous_close")]
    public decimal? PreviousClose { get; set; }

    [JsonPropertyName("has_options")]
    public bool HasOptions { get; set; }

    [JsonPropertyName("time")]
    public long? Time { get; set; }
}