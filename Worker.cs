using Hackernews_Fetcher.Rmq.Publisher;
using Hackernews_Fetcher.Services;

namespace Hackernews_Fetcher;

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
        _logger.LogInformation("Starting fetching...");

        while(!stoppingToken.IsCancellationRequested)
        {
            await foreach (var story in _apiConnector.GetNewStories().WithCancellation(stoppingToken))
            {
                if (story is not null) 
                {
                    _publisher.Publish("feed", story);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
