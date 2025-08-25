using InsiderTrade.Helpers;
using System.Text.Json.Serialization;

namespace InsiderTrade.Models;

public sealed record OptionInstrumentDto(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("category")] string Category,         // "CALL" | "PUT"
    [property: JsonPropertyName("strike")] decimal Strike,
    [property: JsonPropertyName("spot_price")] decimal SpotPrice,
    [property: JsonPropertyName("due_date")] DateTime? DueDate,
    [property: JsonPropertyName("close")] decimal Close,
    [property: JsonPropertyName("volume")] long Volume,
    [property: JsonPropertyName("financial_volume")] decimal? FinancialVolume,
    [property: JsonPropertyName("time"), JsonConverter(typeof(FlexibleLongConverter))] long? TimeMs,
    [property: JsonPropertyName("variation")] decimal? Variation,
     [property: JsonPropertyName("high")] decimal? High,
     [property: JsonPropertyName("low")] decimal? Low

)
{
    public DateTimeOffset? TimeBrt =>
        TimeMs.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(TimeMs.Value).ToOffset(TimeSpan.FromHours(-3))
            : null;

    public string VariationFormatted =>
        Variation.HasValue ? $"{Variation.Value:F2}%" : "-";

    public string FinancialVolumeFormatted =>
       FinancialVolume.HasValue ? FinancialVolume.Value.ToString("N2") : "-";

}
