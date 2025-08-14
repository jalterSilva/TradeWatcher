using System.Linq;
using InsiderTrade.Client;
using InsiderTrade.Helper;
using InsiderTrade.Logic;
using InsiderTrade.Options;
using Microsoft.Extensions.Options;

namespace InsiderTrade
{
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

            // ---- Passo 1: imprimir prefixos para ITUB ----
            var opt = _optMon.CurrentValue;
            var underlying = opt.Underlyings.First();             // "ITUB4"
            var root = SeriesHelper.OptionRoot(underlying);       // "ITUB"

            var calls = SeriesHelper.TakeSeries([.. opt.Series.Calls], opt.Series.CurrentCall, 3).ToList(); // H,I,J
            var puts = SeriesHelper.TakeSeries([.. opt.Series.Puts], opt.Series.CurrentPut, 3).ToList(); // T,U,V

            var prefixes = calls.Select(s => $"{root}{s}")
                                .Concat(puts.Select(s => $"{root}{s}"))
                                .ToList();

            _logger.LogInformation("Underlying: {Underlying}  Root: {Root}", underlying, root);
            _logger.LogInformation("CALL series: {Calls}", string.Join(",", calls));
            _logger.LogInformation("PUT  series: {Puts}", string.Join(",", puts));
            _logger.LogInformation("Prefixes: {Prefixes}", string.Join(", ", prefixes));
            // ------------------------------------------------

            // ---- Passo 2: Replay do dia OU Live ----
            const bool REPLAY_TODAY = true;  // ← Troque para false para rodar em modo LIVE
            var minutes = _optMon.CurrentValue.CandleMinutes;  // ex.: 15
            var testSymbol = "ITUBI398";                          // símbolo de teste (fixo por enquanto)

            if (REPLAY_TODAY)
            {
                // REPLAY: percorre TODAS as janelas do pregão de hoje (10:00→17:00), alinhadas ao step
                foreach (var (fromBrt, toBrt) in TimeHelper.SessionWindowsBrt(minutes))
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    try
                    {
                        _logger.LogInformation("Replay janela BRT: {From}..{To}",
                            TimeHelper.ToOpLabString(fromBrt), TimeHelper.ToOpLabString(toBrt));

                        var json = await _oplab.GetHistoricalOptionRawAsync(
                            testSymbol, minutes, fromBrt, toBrt, stoppingToken);

                        _logger.LogInformation("Replay {Symbol}/{Min}m [{From}..{To}]: {Json}",
                            testSymbol, minutes,
                            TimeHelper.ToOpLabString(fromBrt),
                            TimeHelper.ToOpLabString(toBrt),
                            json);

                        // Pequeno respiro para não estourar limite
                        await Task.Delay(60, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro no replay {From}->{To}: {Msg}",
                            TimeHelper.ToOpLabString(fromBrt),
                            TimeHelper.ToOpLabString(toBrt),
                            ex.Message);
                    }
                }

                // encerra após o replay
                return;
            }
            
        }
    }
}
