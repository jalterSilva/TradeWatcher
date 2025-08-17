namespace InsiderTrade.Options;

public sealed class OpLabOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v3";
    public string AccessToken { get; set; } = string.Empty;
}
