using InsiderTrade.Client;
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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var opt = _optMon.CurrentValue;
                var minVol = opt.Severity?.Star1Min ?? 500_000;

                foreach (var underlying in opt.Underlyings)
                {

                    var options = await _oplab.GetAllOptionsAsync(underlying, stoppingToken);

                    var filtered = options
                     .Where(o =>
                         (o.Volume >= minVol) // regra padrão
                         || (o.DaysToMaturity >= 60 && o.Volume >= 30_000) // regra especial longas
                     )
                     .OrderBy(o => (o.DaysToMaturity >= 60 && o.Volume >= 30_000)) // longas vão pro fim
                     .ThenByDescending(o => o.Volume) // dentro do grupo, ordena por volume
                     .ToList();

                    // 👉 Se não tiver nenhum registro que atenda ao critério, pula este ativo.
                    if (filtered.Count == 0)
                        continue;


                    // Vinheta de separação entre ativos
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(new string('-', 90));
                    Console.WriteLine($"### PROCESSANDO ATIVO: {underlying}  ");
                    Console.WriteLine($"### Total encontradas (Vol > {minVol:N0}): {filtered.Count}");
                    Console.WriteLine(new string('-', 90));
                    Console.ResetColor();

                    foreach (var o in filtered)
                    {
                        // Converte timestamp vindo da API para BRT (UTC-3)
                        DateTime t;
                        if (o.TimeMs is not null and > 0)
                        {
                            t = DateTimeOffset
                                    .FromUnixTimeMilliseconds(o.TimeMs.Value) // sempre UTC
                                    .UtcDateTime                                // garante UTC puro
                                    .AddHours(-3);                              // converte para BRT
                        }
                        else
                        {
                            t = DateTime.Now; // fallback
                        }

                        // Define cor de acordo com volume
                        // 🎯 Nova lógica de cor (prioridade para opções longas)
                        if (o.DaysToMaturity >= 60 && o.Volume >= 30_000)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                        }
                        else if (o.Volume >= 2_000_000)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else if (o.Volume >= 1_000_001)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        else if (o.Volume >= 500_000)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }

                        Console.WriteLine(
                            $"Dias Venc.= {o.DaysToMaturity}| Spot= {o.SpotPrice}| Opção= {o.Symbol,-10}| {o.Category,-4}| Strike= {o.Strike}| Vol= {o.Volume:N0}| Vol. Fin= {o.FinancialVolumeFormatted}| Baixa= {o.Low}| Alta= {o.High}| Variacao= {o.VariationFormatted}"
                        );

                        Console.ResetColor();

                    }

                    Console.WriteLine();
                    Console.WriteLine();
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
