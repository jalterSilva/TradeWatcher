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
                _logger.LogInformation(
                    "[{AlertLevel}] {Symbol} | {Direction} | Open: {Open:F2} | Last: {LastPrice:F2} | Change: {ChangePercent:F2}%",
                    alert.AlertLevel,
                    alert.Symbol,
                    alert.Direction,
                    alert.Open,
                    alert.LastPrice,
                    alert.ChangePercent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar o StockWatcher.");
        }
    }
}