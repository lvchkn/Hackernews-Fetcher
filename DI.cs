using System.Reflection;
using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Repos;
using Hackernews_Fetcher.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using RabbitMQ.Client;

namespace Hackernews_Fetcher;

public static class DI
{
    private static IConfiguration _configuration = default!;
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        _configuration = configuration;
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddRabbitMq();
        services.AddMongoDb();
        services.AddHttp();

        services.AddSingleton<IApiConnector, ApiConnector>();
        services.AddSingleton<ICommentsRepository, CommentsRepository>();
        services.AddSingleton<IStoriesRepository, StoriesRepository>();
        services.AddHostedService<Worker>();
        
        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        var rmqHostname = _configuration.GetValue<string>("RabbitMq:Hostname");
        var rmqPort = _configuration.GetValue<int>("RabbitMq:Port");
        var rmqUsername = _configuration.GetValue<string>("RabbitMq:Username");
        var rmqPassword = _configuration.GetValue<string>("RabbitMq:Password");
        
        var connectionString = $"amqp://{rmqUsername}:{rmqPassword}@{rmqHostname}:{rmqPort}";

        services.AddSingleton<IConnectionFactory>(new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            VirtualHost = "/"
        });

        return services;
    }

    private static IServiceCollection AddHttp(this IServiceCollection services)
    {
        var hackernewsApiUrl = _configuration.GetValue<string>("HackernewsApi:Url");

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

    private static IServiceCollection AddMongoDb(this IServiceCollection services)
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
            cm.SetIgnoreExtraElements(true);
        });

        services.Configure<MongoSettings>(_configuration.GetSection("MongoDb"));

        return services;
    }
}