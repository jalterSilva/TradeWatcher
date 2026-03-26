using StockWatcher.Services;

namespace StockWatcher.Workers;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly StockWatcherService _stockWatcherService;

    public Worker(ILogger<Worker> logger, StockWatcherService stockWatcherService)
    {
        _logger = logger;
        _stockWatcherService = stockWatcherService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando StockWatcher...");

        try
        {
            var alerts = await _stockWatcherService.GetTriggeredAlertsAsync(stoppingToken);

            if (alerts.Count == 0)
            {
                _logger.LogInformation("Nenhum ativo atingiu 3%, 5% ou 10%.");
                return;
            }

            foreach (var alert in alerts)
            {
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = alert.Direction == "UP"
                    ? ConsoleColor.Green
                    : ConsoleColor.Yellow;

                Console.WriteLine(
                    "[{0}] {1} | {2} | Open: {3:F2} | Last: {4:F2} | Change: {5:F2}%",
                    alert.AlertLevel,
                    alert.Symbol,
                    alert.Direction,
                    alert.Open,
                    alert.LastPrice,
                    alert.ChangePercent);

                Console.WriteLine();

                Console.ForegroundColor = originalColor;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar o StockWatcher.");
        }
    }
}