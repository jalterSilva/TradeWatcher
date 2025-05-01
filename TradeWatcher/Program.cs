using TradeWatcher;
using TradeWatcher.Service;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<BrapiService>();

// registra o worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
