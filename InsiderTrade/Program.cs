using InsiderTrade;
using InsiderTrade.Client;
using InsiderTrade.Options;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Config: appsettings + user-secrets + env
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// Bind das Options
builder.Services.Configure<InsiderTradeOptions>(builder.Configuration.GetSection("InsiderTrade"));
builder.Services.Configure<OpLabOptions>(builder.Configuration.GetSection("OpLab"));

// HttpClient tipado para OpLab
builder.Services.AddHttpClient<OpLabClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<OpLabOptions>>().Value;

    http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(30);

    if (!string.IsNullOrWhiteSpace(opt.AccessToken))
        http.DefaultRequestHeaders.Add("Access-Token", opt.AccessToken);
});

// Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
