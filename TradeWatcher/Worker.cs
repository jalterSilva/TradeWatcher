using TradeWatcher.Logic;
using TradeWatcher.Service;

namespace TradeWatcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly BrapiService _brapiService;
        private readonly BollingerAnalyzer _bollingerAnalyzer;
        private readonly MarketDataService _marketDataService;
        private readonly TelegramService _telegramService;
        private readonly List<string> _tickers;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _brapiService = new BrapiService(new HttpClient());
            _bollingerAnalyzer = new BollingerAnalyzer();
            _marketDataService = new MarketDataService();
            _telegramService = new TelegramService(configuration);

            // Carrega tickers do appsettings.json
            _tickers = configuration.GetSection("TickersToMonitor").Get<List<string>>() ?? new List<string>();

            // LOG opcional: mostrar todos os ativos carregados no início
            _logger.LogInformation($" Ativos monitorados: {string.Join(", ", _tickers)}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await PrintLog($" Bom dia pessoal, mais um dia de alertas com o Trade Watcher, lembrando que estamos trabalhando com 3 BANDAS DE BOLLINGER, sendo as 2 mais profundas importantes. ");
            await PrintLog($" CONFIGURAÇÃO: BANDA 1 200 PERÍODOS DESVIO PADRÃO DE 2. ");
            await PrintLog($" CONFIGURAÇÃO: BANDA 2 200 PERÍODOS DESVIO PADRÃO DE 3. ");
            await PrintLog($" CONFIGURAÇÃO: BANDA 3 200 PERÍODOS DESVIO PADRÃO DE 4. ");
            await PrintLog($" Bons trades a todos");

            foreach (var ticker in _tickers)
            {
                try
                {
                    _logger.LogInformation($"[INFO] Buscando candles para {ticker}...");

                    // Brapi api
                    //var candles = await _brapiService.GetDailyCandlesAsync(ticker);

                    // yahoo finance api
                    var candles = await _marketDataService.GetMarketDailyCandlesAsync(ticker);

                    if (candles == null || candles.Count == 0)
                    {
                        _logger.LogWarning($"[WARNING] Nenhum candle retornado para {ticker}.");
                        continue;
                    }

                    _logger.LogInformation($"[INFO] {candles.Count} candles carregados para {ticker}.");

                    var resultado = _bollingerAnalyzer.Analyze(candles, ticker);

                    if (resultado.TouchedBand2Upper)
                        await PrintLog($"{ticker} TOCOU BANDA 2 SUPERIOR!");
                    else if (resultado.NearBand2Upper)
                        await PrintLog($"{ticker} APROXIMOU DA BANDA 2 SUPERIOR!");

                    if (resultado.TouchedBand2Lower)
                        await PrintLog($"{ticker} TOCOU BANDA 2 INFERIOR!");
                    else if (resultado.NearBand2Lower)
                        await PrintLog($"{ticker} APROXIMOU DA BANDA 2 INFERIOR!");

                    if (resultado.TouchedBand3Upper)
                        await PrintLog($"{ticker} TOCOU BANDA 3 SUPERIOR!");
                    else if (resultado.NearBand3Upper)
                        await PrintLog($"{ticker} APROXIMOU DA BANDA 3 SUPERIOR!");

                    if (resultado.TouchedBand3Lower)
                        await PrintLog($"{ticker} TOCOU BANDA 3 INFERIOR!");
                    else if (resultado.NearBand3Lower)
                        await PrintLog($"{ticker} APROXIMOU DA BANDA 3 INFERIOR!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[ERRO] Erro ao processar o ativo {ticker}.");
                }
            }

            await PrintLog("Análise concluída. TradeWatcher finalizado.");
            _logger.LogInformation($"[INFO] Análise concluída. TradeWatcher finalizado.");

        }

        private async Task PrintLog(string message)
        {
            if (message.Contains("BANDA"))
            {
                Console.ForegroundColor = ConsoleColor.Blue; // ou Red, Cyan etc
                Console.WriteLine(message);
                Console.ResetColor();
            }

            await _telegramService.SendMessageAsync(message); // Enviar pro Telegram também
        }
    }
}

