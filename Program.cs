using HackernewsFetcher;
using Services;
using DI;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.development.json")
    .AddEnvironmentVariables()
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddRabbit(configuration);

        services.AddHttp(configuration);

        services.AddSingleton<IApiConnector, ApiConnector>();
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
