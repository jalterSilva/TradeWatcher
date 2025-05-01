using System.Text.Json;
using System.Text.Json.Serialization;
using TradeWatcher.Model;

namespace TradeWatcher.Service
{
    public class BrapiService
    {
        private readonly HttpClient _httpClient;
        const string TOKEN = "1BFAaH3e8FgKHh88HbPpZE";

        public BrapiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Candle>> GetDailyCandlesAsync(string ticker, string range = "3mo", string interval = "1d")
        {
            var url = $"https://brapi.dev/api/quote/{ticker.ToUpper()}?range={range}&interval={interval}&token={TOKEN}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao acessar a API da Brapi: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BrapiResponse>(json);

            return result?.Results?.FirstOrDefault()?.HistoricalDataPrice
                .Where(c => c.AdjClose.HasValue)
                .Select(c => new Candle
                {
                    Date = DateTimeOffset.FromUnixTimeSeconds(c.Date).DateTime,
                    Open = c.Open.HasValue ? c.Open.Value : 0,
                    High = c.High.HasValue ? c.High.Value : 0,
                    Low = c.Low.HasValue ? c.Low.Value : 0,
                    Close = c.AdjClose.HasValue ? c.AdjClose.Value : 0
                })
                .ToList() ?? new List<Candle>();
        }


        public class BrapiResponse
        {
            [JsonPropertyName("results")]
            public List<BrapiResult>? Results { get; set; }
        }

        public class BrapiResult
        {
            [JsonPropertyName("historicalDataPrice")]
            public required List<BrapiCandle> HistoricalDataPrice { get; set; }
        }

        public class BrapiCandle
        {
            [JsonPropertyName("date")]
            public long Date { get; set; }

            [JsonPropertyName("open")]
            public decimal? Open { get; set; }

            [JsonPropertyName("high")]
            public decimal? High { get; set; }

            [JsonPropertyName("low")]
            public decimal? Low { get; set; }

            [JsonPropertyName("adjustedClose")]
            public decimal? AdjClose { get; set; } 
        }
    }
}

