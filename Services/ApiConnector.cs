using System.Text.Json;
using Models;
using Repos.StoriesRepository;

namespace Services;

public class ApiConnector : IApiConnector
{
    private readonly IStoriesRepository _storiesRepo;
    private readonly ILogger<ApiConnector> _logger;
    private readonly HttpClient _client;
    private const string ApiUrlItem = "item/{0}.json";
    private const string ApiUrlNewStories = "newstories.json";
    private const int MaxStoriesInBatch = 50;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiConnector(
        ILogger<ApiConnector> logger, 
        IHttpClientFactory clientFactory,
        IStoriesRepository storiesRepo)
    {
        _logger = logger;
        _client = clientFactory.CreateClient("ApiV0");
        _storiesRepo = storiesRepo;
    }

    public async Task<int[]?> GetTopStoryIds(CancellationToken token)
    {
        var response = await _client.GetAsync(ApiUrlNewStories);
        var responseString = await response.Content.ReadAsStringAsync();

        var storyIds = JsonSerializer.Deserialize<int[]>(responseString, _jsonSerializerOptions) ?? null;

        return storyIds;
    }

    public async IAsyncEnumerable<StoryDto?> GetTopStories()
    {
        var token = new CancellationTokenSource().Token;
        var storyIds = await GetTopStoryIds(token);

        if (storyIds is null)
        {
            yield return null;
        }

        int lastSavedId;
        try
        {
           lastSavedId = await _storiesRepo.GetBiggestIdAsync();
        }
        catch (Exception)
        {
            lastSavedId = storyIds!.Min() - 1; // fallback in case db is empty
        }
         
        var newIds = storyIds!.Where(id => id > lastSavedId)
            .OrderBy(id => id)
            .Take(MaxStoriesInBatch);

        foreach (var id in newIds)
        {
            var response = await _client.GetAsync(string.Format(ApiUrlItem, id));
            var responseString = await response.Content.ReadAsStringAsync();
            var storyDto = JsonSerializer.Deserialize<StoryDto>(responseString, _jsonSerializerOptions);

            if (storyDto is null) continue;

            await _storiesRepo.AddAsync(storyDto);

            yield return storyDto;
        }
    }
}