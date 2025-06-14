using TradeWatcher.Model;

namespace TradeWatcher.Logic
{
    public class BollingerAnalyzer
    {
        private const int Period = 200;

        public BollingerResult Analyze(List<Candle> candles, string ticker)
        {
            if (candles == null)
                throw new InvalidOperationException($"Não existe candles para o ativo {ticker}");

            var orderedCandles = candles.OrderBy(c => c.Date).ToList();
            var lastCandle = orderedCandles.Last();

            // Últimos 200 fechamentos
            var closes = orderedCandles
             .Where(c => c.Close > 0) // candle válido
             .Select(c => c.Close)
             .TakeLast(Period)
             .ToList();

            // Logar o último candle que estamos usando
            Console.WriteLine($"[DEBUG] Último fechamento: {lastCandle.Close}, Data: {lastCandle.Date}");


            // Calcula Média Aritmética
            var mean = closes.Average();

            var closesAsDouble = closes.Select(c => (double?)c).ToList();
            var meanDouble = (double)(mean ?? 0);
            var variance = closesAsDouble.Sum(c => Math.Pow((double)(c - meanDouble ?? 0), 2)) / (closesAsDouble.Count - 1);

            var stdDev = (decimal)Math.Sqrt((double)variance!);


            // Calcula as bandas        
            var upperBand2 = mean + (3 * stdDev);
            var lowerBand2 = mean - (3 * stdDev);
            var upperBand3 = mean + (4 * stdDev);
            var lowerBand3 = mean - (4 * stdDev);

            // Detecta toques
            const decimal proximityThreshold = 0.10m; // 10 centavos de margem
            var close = lastCandle.Close;

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
                LastClose = close,
                UpperBand2 = Math.Round(upperBand2 ?? 0, 2),
                LowerBand2 = Math.Round(lowerBand2 ?? 0, 2),
                UpperBand3 = Math.Round(upperBand3 ?? 0, 2),
                LowerBand3 = Math.Round(lowerBand3 ?? 0, 2),
                TouchedBand2Upper = close >= upperBand2,
                TouchedBand2Lower = close <= lowerBand2,
                TouchedBand3Upper = close >= upperBand3,
                TouchedBand3Lower = close <= lowerBand3,
                NearBand2Upper = close < upperBand2 && close >= (upperBand2 - proximityThreshold),
                NearBand2Lower = close > lowerBand2 && close <= (lowerBand2 + proximityThreshold),
                NearBand3Upper = close < upperBand3 && close >= (upperBand3 - proximityThreshold),
                NearBand3Lower = close > lowerBand3 && close <= (lowerBand3 + proximityThreshold)
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
