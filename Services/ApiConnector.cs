using System.Text.Json;
using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Repos;

namespace Hackernews_Fetcher.Services;

public class ApiConnector : IApiConnector
{
    private readonly IStoriesRepository _storiesRepo;
    private readonly ILogger<ApiConnector> _logger;
    private readonly HttpClient _httpClient;

    public ApiConnector(IHttpClientFactory clientFactory,
        IStoriesRepository storiesRepo,
        ILogger<ApiConnector> logger)
    {
        _httpClient = clientFactory.CreateClient("ApiV0");
        _storiesRepo = storiesRepo;
        _logger = logger;
    }

    private async Task<ApiResponse?> MakeRequestToApi(int timeThreshold, 
        int pointsThreshold,
        int pageNumber,
        int pageSize,
        string tags,
        string query = "")
    {
        var requestUri = $"search?query={query}" +
                         $"&tags={tags}" +
                         $"&page={pageNumber}" +
                         $"&hitsPerPage={pageSize}" +
                         $"&numericFilters=points>{pointsThreshold}," +
                         $"created_at_i>{timeThreshold}";
        
        var response = await _httpClient.GetAsync(requestUri);
        var responseString = await response.Content.ReadAsStringAsync();

        var apiResponseObject = JsonSerializer.Deserialize<ApiResponse>(responseString);

        return apiResponseObject;
    }

    private async Task<List<StoryHnDto>> GetStories(int timeThreshold, int pointsThreshold, string query = "")
    {
        var stories = new List<StoryHnDto>();
        
        var apiResponseObject = await MakeRequestToApi(timeThreshold,
            pointsThreshold,
            0,
            100,
            "story",
            query);

        if (apiResponseObject is null || apiResponseObject.Stories.Length == 0)
        {
            return stories;
        }

        if (apiResponseObject.NumberOfPages < 2)
        {
            return apiResponseObject.Stories.ToList();
        }

        var requestsLimit = Math.Min(apiResponseObject.NumberOfPages, 5_000);
        
        for (var i = 1; i <= requestsLimit; i++)
        {
            var nextPageResponse = await MakeRequestToApi(timeThreshold, 
                pointsThreshold,
                i,
                100,
                "story",
                query);
            
            if (nextPageResponse is null) continue;
            
            stories.AddRange(nextPageResponse.Stories);
        }
        
        return stories;
    }

    public async IAsyncEnumerable<StoryHnDto?> GetNewStories()
    {
        var timeThreshold = await _storiesRepo.GetLatestTimestampAsync();
        const int pointsThreshold = 3;
        //1700067699
        var storyDtos = await GetStories(timeThreshold, pointsThreshold);

        foreach (var storyDto in storyDtos)
        {
            await _storiesRepo.AddAsync(storyDto);
            _logger.LogInformation($"Story added: {storyDto}");
            yield return storyDto;
        }
    }
}