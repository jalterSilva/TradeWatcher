using System.Text.Json;
using TradeWatcher.Model;

namespace TradeWatcher.Service
{
    public class MarketDataService
    {

        private readonly HttpClient _httpClient;

        public MarketDataService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");
        }

        public async Task<List<Candle>> GetMarketDailyCandlesAsync(string ticker)
        {

            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}.SA?interval=1d&range=1y";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<YahooChartResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.Chart?.Result == null || !data.Chart.Result.Any())
                return new List<Candle>();

            var result = data.Chart.Result.First();
            var timestamps = result.Timestamp;
            var quotes = result?.Indicators?.Quote?.First();

            var candles = new List<Candle>();

            for (int i = 0; i < timestamps?.Count; i++)
            {
                candles.Add(new Candle
                {
                    Date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]),
                    Open = (decimal?)quotes?.Open?[i],
                    High = (decimal?)quotes?.High?[i],
                    Low = (decimal?)quotes?.Low?[i],
                    Close = (decimal?)quotes?.Close?[i]
                });
            }

            return candles;
        }
    }
}
