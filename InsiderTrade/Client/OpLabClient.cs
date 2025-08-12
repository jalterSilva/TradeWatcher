using System.Net.Http;
using InsiderTrade.Options;
using Microsoft.Extensions.Options;
using InsiderTrade.Logic;

namespace InsiderTrade.Client
{
    public sealed class OpLabClient
    {
        private readonly HttpClient _http;
        private readonly string _apiVersion;

        public OpLabClient(HttpClient http, IOptions<OpLabOptions> opt)
        {
            _http = http;
            _apiVersion = (opt.Value.ApiVersion ?? "v3").Trim();
        }

        // Retorna o JSON cru do último candle FECHADO (janela em BRT)
        public async Task<string> GetLastCandleRawAsync(string symbol, int minutes, CancellationToken ct = default)
        {
            (DateTime fromBrt, DateTime toBrt) = TimeHelper.LastClosedWindowBrt(minutes);
            var fromStr = TimeHelper.ToOpLabString(fromBrt);
            var toStr = TimeHelper.ToOpLabString(toBrt);

            var path = $"/{_apiVersion}/market/historical/{symbol}/{minutes}?from={fromStr}&to={toStr}";
            using var resp = await _http.GetAsync(path, ct);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadAsStringAsync(ct);
        }
    }
}
