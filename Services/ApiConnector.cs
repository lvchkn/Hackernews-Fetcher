using System.Text.Json;
using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Repos;

namespace Hackernews_Fetcher.Services;

public class ApiConnector : IApiConnector
{
    private readonly IStoriesRepository _storiesRepo;
    private readonly HttpClient _httpClient;
    
    private const string ApiUrlItem = "item/{0}.json";
    private const string ApiUrlNewStories = "newstories.json";
    private const string ApiUrlTopStories = "topstories.json";
    
    private const int MaxStoriesInBatch = 50;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiConnector(IHttpClientFactory clientFactory, IStoriesRepository storiesRepo)
    {
        _httpClient = clientFactory.CreateClient("ApiV0");
        _storiesRepo = storiesRepo;
    }

    private async Task<int[]?> GetStoriesIds(string apiUrl)
    {
        var response = await _httpClient.GetAsync(apiUrl);
        var responseString = await response.Content.ReadAsStringAsync();

        var storyIds = JsonSerializer.Deserialize<int[]>(responseString, _jsonSerializerOptions) ?? null;

        return storyIds;
    }

    private async Task<StoryDto?> GetStory(int id)
    {
        var response = await _httpClient.GetAsync(string.Format(ApiUrlItem, id));
        var responseString = await response.Content.ReadAsStringAsync();
        var storyDto = JsonSerializer.Deserialize<StoryDto>(responseString, _jsonSerializerOptions);

        return storyDto;
    }

    public async IAsyncEnumerable<StoryDto?> GetTopStories()
    {
        var storyIds = await GetStoriesIds(ApiUrlTopStories);
        
        if (storyIds is null)
        {
            yield return null;
        }

        foreach (var id in storyIds!)
        {
            var storyDto = await GetStory(id);
            yield return storyDto;
        }
    }

    public async IAsyncEnumerable<StoryDto?> GetNewStories()
    {
        var storyIds = await GetStoriesIds(ApiUrlNewStories);

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
            var fallback = storyIds!.Min() - 1;
            lastSavedId = fallback;
        }
         
        var newIds = storyIds!.Where(id => id > lastSavedId)
            .OrderBy(id => id)
            .Take(MaxStoriesInBatch);

        foreach (var id in newIds)
        {
            var storyDto = await GetStory(id);

            if (storyDto is null) continue;

            await _storiesRepo.AddAsync(storyDto);

            yield return storyDto;
        }
    }
}