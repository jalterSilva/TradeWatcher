using TradeWatcher.Model;

namespace TradeWatcher.Logic
{
    public class BollingerAnalyzer
    {
        private const int Period = 60;

        public BollingerResult Analyze(List<Candle> candles, string ticker)
        {
            if (candles == null)
                throw new InvalidOperationException($"Não existe candles para o ativo {ticker}");

            var orderedCandles = candles.OrderBy(c => c.Date).ToList();
            var lastCandle = orderedCandles.Last();

            // Últimos 60 fechamentos
            var closes = orderedCandles
             .Where(c => c.Close > 0) // candle válido
             .Select(c => c.Close)
             .TakeLast(Period)
             .ToList();

            // Logar o último candle que estamos usando
            Console.WriteLine($"[DEBUG] Último fechamento: {lastCandle.Close}, Data: {lastCandle.Date}");


            // Calcula Média Aritmética
            var mean = closes.Average();

            // Calcula Variância
            var variance = closes.Sum(c => (c - mean) * (c - mean)) / (closes.Count);

            // Calcula Desvio Padrão
            var stdDev = (decimal)Math.Sqrt((double)variance!);


            // Calcula as bandas
            var upperBand1 = mean + (2 * stdDev);
            var lowerBand1 = mean - (2 * stdDev);

            var upperBand2 = mean + (3 * stdDev);
            var lowerBand2 = mean - (3 * stdDev);

            var upperBand3 = mean + (4 * stdDev);
            var lowerBand3 = mean - (4 * stdDev);


            // Detecta toques
            const decimal proximityThreshold = 0.10m; // 10 centavos de margem

            bool touchedBand2Upper = lastCandle.Close >= upperBand2;
            bool touchedBand2Lower = lastCandle.Close <= lowerBand2;
            bool touchedBand3Upper = lastCandle.Close >= upperBand3;
            bool touchedBand3Lower = lastCandle.Close <= lowerBand3;

            bool nearBand2Upper = !touchedBand2Upper && lastCandle.Close >= (upperBand2 - proximityThreshold);
            bool nearBand2Lower = !touchedBand2Lower && lastCandle.Close <= (lowerBand2 + proximityThreshold);
            bool nearBand3Upper = !touchedBand3Upper && lastCandle.Close >= (upperBand3 - proximityThreshold);
            bool nearBand3Lower = !touchedBand3Lower && lastCandle.Close <= (lowerBand3 + proximityThreshold);

            return new BollingerResult
            {
                Date = lastCandle.Date.DateTime,
                LastClose = lastCandle.Close,
                UpperBand2 = Math.Round(upperBand2 ?? 0m, 2),
                LowerBand2 = Math.Round(lowerBand2 ?? 0m, 2),
                UpperBand3 = Math.Round(upperBand3 ?? 0m, 2),
                LowerBand3 = Math.Round(lowerBand3 ?? 0m, 2),
                TouchedBand2Upper = touchedBand2Upper,
                TouchedBand2Lower = touchedBand2Lower,
                TouchedBand3Upper = touchedBand3Upper,
                TouchedBand3Lower = touchedBand3Lower,
                NearBand2Upper = nearBand2Upper,
                NearBand2Lower = nearBand2Lower,
                NearBand3Upper = nearBand3Upper,
                NearBand3Lower = nearBand3Lower
            };
        }

    }

    public class BollingerResult
    {
        public DateTime Date { get; set; }
        public decimal? LastClose { get; set; }
        public decimal? UpperBand2 { get; set; }
        public decimal? LowerBand2 { get; set; }
        public decimal? UpperBand3 { get; set; }
        public decimal? LowerBand3 { get; set; }
        public bool TouchedBand2Upper { get; set; }
        public bool TouchedBand2Lower { get; set; }
        public bool TouchedBand3Upper { get; set; }
        public bool TouchedBand3Lower { get; set; }
        public bool NearBand2Upper { get; set; }
        public bool NearBand2Lower { get; set; }
        public bool NearBand3Upper { get; set; }
        public bool NearBand3Lower { get; set; }
    }
}
