using Microsoft.Extensions.Options;
using StockWatcher.Configuration;
using StockWatcher.Services;
using StockWatcher.Workers;
using StockWatcher.Clients;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OpLabOptions>(
    builder.Configuration.GetSection("OpLab"));

builder.Services.AddHttpClient<OpLabClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OpLabOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl.EndsWith("/")
        ? options.BaseUrl
        : options.BaseUrl + "/");

    client.DefaultRequestHeaders.Add("Access-Token", options.AccessToken);
});

builder.Services.AddSingleton<StockWatcherService>();
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();