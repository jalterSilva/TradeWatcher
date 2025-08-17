using InsiderTrade.Client;
using InsiderTrade.Models;
using InsiderTrade.Options;
using Microsoft.Extensions.Options;

namespace InsiderTrade;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly OpLabClient _oplab;
    private readonly IOptionsMonitor<InsiderTradeOptions> _optMon;

    public Worker(ILogger<Worker> logger, OpLabClient oplab, IOptionsMonitor<InsiderTradeOptions> optMon)
    {
        _logger = logger;
        _oplab = oplab;
        _optMon = optMon;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InsiderTrade.Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var opt = _optMon.CurrentValue;
                var underlying = opt.Underlyings.First(); // exemplo: BBDC4

                _logger.LogInformation("Buscando opções para {Underlying}", underlying);

                var options = await _oplab.GetAllOptionsAsync(underlying, stoppingToken);

                // pega limite mínimo de volume da config
                var minVol = opt.Severity?.Star1Min ?? 500_000;

                var filtered = options
                    .Where(o => o.Volume >= 500_000)     // só pega acima de 499k
                    .OrderByDescending(o => o.Volume)    // ordena do maior pro menor
                    .ToList();

                _logger.LogInformation("Total encontradas (Vol > {MinVol:N0}): {Count}", minVol, filtered.Count);


                // Vinheta de separação entre ativos
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(new string('-', 90));
                Console.WriteLine($"### PROCESSANDO ATIVO: {underlying}  ");
                Console.WriteLine(new string('-', 90));
                Console.ResetColor();

                foreach (var o in filtered)
                {
                    var t = DateTimeOffset.FromUnixTimeMilliseconds(o.TimeMs ?? 0).ToOffset(TimeSpan.FromHours(-3));

                    // Define cor de acordo com volume
                    if (o.Volume >= 2_000_000)
                        Console.ForegroundColor = ConsoleColor.Red;       // acima de 2M
                    else if (o.Volume >= 1_000_001)
                        Console.ForegroundColor = ConsoleColor.Yellow;    // 1M a 2M
                    else if (o.Volume >= 500_000)
                        Console.ForegroundColor = ConsoleColor.Blue;      // 500K a 1M

                    Console.WriteLine(
                        $"{t:yyyy-MM-dd} | Ativo= {underlying} | Spot= {o.SpotPrice} | Opção= {o.Symbol,-10} | {o.Category,-4} | Strike= {o.Strike} | Vol= {o.Volume:N0}"
                    );

                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Worker: {Msg}", ex.Message);
            }

            // aguarda intervalo configurado
            await Task.Delay(TimeSpan.FromMinutes(_optMon.CurrentValue.IntervalMinutes), stoppingToken);
        }
    }
}
