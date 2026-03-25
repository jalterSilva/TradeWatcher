using StockWatcher.Clients;
using StockWatcher.Models;

namespace StockWatcher.Services;

public sealed class StockWatcherService
{
    private readonly OpLabClient _opLabClient;

    public StockWatcherService(OpLabClient opLabClient)
    {
        _opLabClient = opLabClient;
    }

    public async Task<IReadOnlyList<StockAlert>> GetTriggeredAlertsAsync(CancellationToken cancellationToken)
    {
        var stocks = await _opLabClient.GetStocksAsync(cancellationToken);

        var alerts = new List<StockAlert>();

        foreach (var stock in stocks)
        {
            if (string.IsNullOrWhiteSpace(stock.Symbol))
                continue;

            if (stock.Close is null || stock.Variation is null)
                continue;

            var changePercent = stock.Variation.Value;
            var absChange = Math.Abs(changePercent);

            var alertLevel = GetAlertLevel(absChange);
            if (string.IsNullOrEmpty(alertLevel))
                continue;

            alerts.Add(new StockAlert
            {
                Symbol = stock.Symbol,
                Name = stock.Name,
                Open = stock.Open ?? 0m,
                LastPrice = stock.Close.Value,
                ChangePercent = Math.Round(changePercent, 2),
                AlertLevel = alertLevel,
                Direction = changePercent >= 0 ? "UP" : "DOWN"
            });
        }

        return alerts
            .OrderByDescending(x => Math.Abs(x.ChangePercent))
            .ToList();
    }

    private static string GetAlertLevel(decimal absChange)
    {
        if (absChange >= 10m) return "10%";
        if (absChange >= 5m) return "5%";
        if (absChange >= 3m) return "3%";

        return string.Empty;
    }
}