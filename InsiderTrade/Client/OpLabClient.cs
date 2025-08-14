using InsiderTrade.Logic;
using InsiderTrade.Options;
using Microsoft.Extensions.Options;

namespace InsiderTrade.Client
{
    public sealed class OpLabClient
    {
        private readonly HttpClient _http;
        private readonly OpLabOptions _opt;

        public OpLabClient(HttpClient http, IOptions<OpLabOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task<string> GetHistoricalOptionRawAsync(
            string symbol, int minutes, DateTime fromBrt, DateTime toBrt, CancellationToken ct = default)
        {
            // mesmo formato que você já usa
            var fromStr = TimeHelper.ToOpLabString(fromBrt); // ex: yyyy-MM-ddTHH:mm
            var toStr = TimeHelper.ToOpLabString(toBrt);

            var path = $"/{_opt.ApiVersion}/market/historical/{symbol}/{minutes}?from={fromStr}&to={toStr}";
            using var resp = await _http.GetAsync(path, ct);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadAsStringAsync(ct);
        }

        // Conveniência: usa a última janela fechada de X minutos em BRT
        public async Task<string> GetHistoricalOptionRawAsync(
            string symbol, int minutes, CancellationToken ct = default)
        {
            var (fromBrt, toBrt) = TimeHelper.LastClosedWindowBrt(minutes);
            return await GetHistoricalOptionRawAsync(symbol, minutes, fromBrt, toBrt, ct);
        }



    }
}
