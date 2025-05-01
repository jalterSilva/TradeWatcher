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

        private async Task WaitUntilMorning(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var target = now.Date.AddHours(7).AddMinutes(30);
            //var target = now.AddSeconds(3);

            if (now > target)
                target = target.AddDays(1); // Se já passou hoje, programa para amanhã

            var delay = target - now;

            _logger.LogInformation($"[INFO] Aguardando até {target:HH:mm} para iniciar análise...");

            await Task.Delay(delay, stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                await WaitUntilMorning(stoppingToken);

                await PrintLog($" Bom dia pessoal, mais um dia de alertas com o Trade Watcher, lembrando que estamos trabalhando com 3 BANDAS DE BOLLINGER, sendo as 2 mais profundas importantes. ");
                await PrintLog($" CONFIGURAÇÃO: BANDA 1 60 PERÍODOS DESVIO PADRÃO DE 2. ");
                await PrintLog($" CONFIGURAÇÃO: BANDA 2 60 PERÍODOS DESVIO PADRÃO DE 3. ");
                await PrintLog($" CONFIGURAÇÃO: BANDA 3 60 PERÍODOS DESVIO PADRÃO DE 4. ");
                await PrintLog($" Bons trades a todos");

                foreach (var ticker in _tickers)
                {
                    try
                    {
                        _logger.LogInformation($"[INFO] Buscando candles para {ticker}...");

                        var candles = await _brapiService.GetDailyCandlesAsync(ticker);

                        if (candles == null || candles.Count == 0)
                        {
                            _logger.LogWarning($"[WARNING] Nenhum candle retornado para {ticker}.");
                            continue;
                        }

                        _logger.LogInformation($"[INFO] {candles.Count} candles carregados para {ticker}.");

                        var resultado = _bollingerAnalyzer.Analyze(candles);

                        _logger.LogInformation($"Último fechamento {ticker}: {resultado.LastClose}");

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

                _logger.LogInformation($"[INFO] Análise concluída. Aguardando até o próximo dia às 7:30...");
            }

        }

        private async Task PrintLog(string message)
        {
            if (message.Contains("BANDA"))
            {
                Console.ForegroundColor = ConsoleColor.Blue; // ou Red, Cyan etc
                Console.WriteLine(message);
                Console.ResetColor();

                await _telegramService.SendMessageAsync(message); // Enviar pro Telegram também

            }
            else
            {
                _logger.LogInformation(message);
            }
        }
    }
}

