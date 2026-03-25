using System.Text.Json;
using StockWatcher.Models;

namespace StockWatcher.Clients;

public sealed class OpLabClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpLabClient> _logger;

    public OpLabClient(HttpClient httpClient, ILogger<OpLabClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StockResponse>> GetStocksAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("market/stocks", cancellationToken);

        _logger.LogInformation("RequestUri: {RequestUri}", response.RequestMessage?.RequestUri);
        _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erro ao chamar OpLab. Body: {Body}", content);
            return [];
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var stocks = JsonSerializer.Deserialize<List<StockResponse>>(content, options);

        return stocks ?? [];
    }
}