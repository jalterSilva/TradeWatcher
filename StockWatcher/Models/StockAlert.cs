namespace StockWatcher.Models;

public sealed class StockAlert
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Open { get; set; }
    public decimal LastPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public string AlertLevel { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
}