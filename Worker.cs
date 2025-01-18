using System.Text;
using System.Text.Json;
using Hackernews_Fetcher.Services;
using AutoMapper;
using Hackernews_Fetcher.Models;
using RabbitMQ.Client;

namespace Hackernews_Fetcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IApiConnector _apiConnector;
    private readonly IConnectionFactory _connectionFactory;
    private readonly IMapper _mapper;

    public Worker(ILogger<Worker> logger, 
        IApiConnector apiConnector, 
        IConnectionFactory connectionFactory, 
        IMapper mapper)
    {
        _logger = logger;
        _apiConnector = apiConnector;
        _connectionFactory = connectionFactory;
        _mapper = mapper;
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    {
        _logger.LogInformation("Starting fetching...");
        var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        const string exchangeName = "feed";
        const string queueName = "stories";
        const string routingKey = "feed.stories";
        
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(queueName, exclusive: false, durable: true, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queueName, exchangeName, routingKey, cancellationToken: stoppingToken);

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        while(!stoppingToken.IsCancellationRequested)
        {
            await foreach (var storyDto in _apiConnector.GetNewStoriesAsync().WithCancellation(stoppingToken))
            {
                if (storyDto is null) continue;
                
                var serializedMessageBody = JsonSerializer.Serialize(_mapper.Map<StoryPublishDto>(storyDto), jsonSerializerOptions);
                var messageBody = Encoding.UTF8.GetBytes(serializedMessageBody);
                    
                var publishProperties = new BasicProperties
                {
                    Persistent = true, 
                    DeliveryMode = DeliveryModes.Persistent
                };
                    
                await channel.BasicPublishAsync(exchangeName, routingKey, mandatory: true, body: messageBody, basicProperties: publishProperties, cancellationToken: stoppingToken);
            }
            
            await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
        }
    }
}
