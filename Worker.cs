using RabbitConnections.Publisher;
using Services;

namespace HackernewsFetcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IApiConnector _apiConnector;
    private readonly Publisher _publisher;

    public Worker(ILogger<Worker> logger, IApiConnector apiConnector, Publisher publisher)
    {
        _logger = logger;
        _apiConnector = apiConnector;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    { 
        while(!stoppingToken.IsCancellationRequested)
        {
            await foreach (var story in _apiConnector.GetTopStories().WithCancellation(stoppingToken))
            {
                if (story is not null) 
                {
                    _publisher.Publish("feed", story);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
