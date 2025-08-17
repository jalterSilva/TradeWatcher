using System.Text.Json;
using InsiderTrade.Models;
using InsiderTrade.Options;
using Microsoft.Extensions.Options;

namespace InsiderTrade.Client;

public sealed class OpLabClient
{
    private readonly HttpClient _http;
    private readonly OpLabOptions _opt;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public OpLabClient(HttpClient http, IOptions<OpLabOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    // GET /v3/market/options/{underlying}
    public async Task<List<OptionInstrumentDto>> GetAllOptionsAsync(string underlying, CancellationToken ct = default)
    {
        var path = $"{_opt.ApiVersion}/market/options/{underlying}";
        using var resp = await _http.GetAsync(path, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<List<OptionInstrumentDto>>(json, _json) ?? new();
    }


}
