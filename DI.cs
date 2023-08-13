using HackernewsFetcher;
using Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using RabbitConnections;
using RabbitConnections.Publisher;
using RabbitMQ.Client;
using Repos.CommentsRepository;
using Repos.StoriesRepository;
using Services;

namespace DI;

public static class DI
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbit(configuration);
        services.AddHttp(configuration);
        services.AddMongoDb(configuration);
        
        services.AddSingleton<IApiConnector, ApiConnector>();
        services.AddSingleton<ICommentsRepository, CommentsRepository>();
        services.AddSingleton<IStoriesRepository, StoriesRepository>();
        services.AddHostedService<Worker>();

        return services;
    }

    public static IServiceCollection AddRabbit(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitHostname = configuration.GetValue<string>("RabbitMq:Hostname");
        var rabbitPort = configuration.GetValue<int>("RabbitMq:Port");
        var rabbitUsername = configuration.GetValue<string>("RabbitMq:Username");
        var rabbitPassword = configuration.GetValue<string>("RabbitMq:Password");

        Console.WriteLine($"hostname is {rabbitHostname}");
        Console.WriteLine($"rabbitPort is {rabbitPort}");
        Console.WriteLine($"rabbitUsername is {rabbitUsername}");
        Console.WriteLine($"rabbitPassword is {rabbitPassword}");

        services.AddSingleton(_ => new ConnectionFactory()
        {
            HostName = rabbitHostname,
            Port = rabbitPort,
            UserName = rabbitUsername,
            Password = rabbitPassword,
            VirtualHost = "/"
        });

        services.AddSingleton<IChannelFactory, ChannelFactory>();
        services.AddSingleton<ChannelWrapper>();
        services.AddSingleton<Publisher>();

        return services;
    }

    public static IServiceCollection AddHttp(this IServiceCollection services, IConfiguration configuration)
    {
        var hackernewsApiUrl = configuration.GetValue<string>("HackernewsApi:Url");

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(retryCount: 5, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2.0, retryAttempt)));

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(45);

        services
            .AddHttpClient("ApiV0", options =>
            { 
                options.BaseAddress = new Uri(hackernewsApiUrl ?? string.Empty);
                options.Timeout = TimeSpan.FromSeconds(600);
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy);

        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        BsonClassMap.RegisterClassMap<Comment>(cm =>
        {
            cm.AutoMap();
            cm.GetMemberMap(c => c.Id).SetIgnoreIfDefault(true);
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<Story>(cm =>
        {
            cm.AutoMap();
            cm.GetMemberMap(c => c.Id).SetIgnoreIfDefault(true);
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.String));
        });

        services.Configure<MongoSettings>(configuration?.GetSection("MongoDb")!);

        return services;
    }
}