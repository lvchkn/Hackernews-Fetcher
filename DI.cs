using Polly;
using RabbitConnections;
using RabbitConnections.Publisher;
using RabbitMQ.Client;

namespace DI;

public static class DI
{
    public static IServiceCollection AddRabbit(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitHostname = configuration.GetValue<string>("RabbitMq:Hostname");
        var rabbitPort = configuration.GetValue<int>("RabbitMq:Port");
        var rabbitUsername = configuration.GetValue<string>("RabbitMq:Username");
        var rabbitPassword = configuration.GetValue<string>("RabbitMq:Password");

        services.AddSingleton(_ => new ConnectionFactory()
        {
            HostName = rabbitHostname,
            Port = rabbitPort,
            UserName = rabbitUsername,
            Password = rabbitPassword
        });

        services.AddSingleton<IChannelFactory, ChannelFactory>();
        services.AddSingleton<ChannelWrapper>();
        services.AddSingleton<Publisher>();

        return services;
    }

    public static IServiceCollection AddHttp(this IServiceCollection services, IConfiguration configuration)
    {
        var hackernewsApiUrl = configuration.GetValue<string>("HackernewsApi:Url");

        services
            .AddHttpClient("ApiV0", options => 
                options.BaseAddress = new Uri(hackernewsApiUrl ?? string.Empty))
            .AddTransientHttpErrorPolicy(builder => 
                builder.WaitAndRetryAsync(retryCount: 5, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2.0, retryAttempt))));

        return services;
    }
}