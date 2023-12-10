using System.Text.Json;
using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Repos;

namespace Hackernews_Fetcher.Services;

public class ApiConnector : IApiConnector
{
    private readonly IStoriesRepository _storiesRepo;
    private readonly HttpClient _httpClient;

    public ApiConnector(IHttpClientFactory clientFactory,
        IStoriesRepository storiesRepo)
    {
        _httpClient = clientFactory.CreateClient("ApiV0");
        _storiesRepo = storiesRepo;
    }

    private async Task<ApiResponse?> MakeRequestAsync(int timeThreshold, 
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

    private async Task<List<StoryHnDto>> GetStoriesAsync(int timeThreshold, int pointsThreshold, string query = "")
    {
        var stories = new List<StoryHnDto>();
        
        var apiResponseObject = await MakeRequestAsync(timeThreshold,
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
            var nextPageResponse = await MakeRequestAsync(timeThreshold, 
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

    public async IAsyncEnumerable<StoryHnDto?> GetNewStoriesAsync()
    {
        var timeThreshold = await _storiesRepo.GetLatestTimestampAsync();
        const int pointsThreshold = 3;

        var storyDtos = await GetStoriesAsync(timeThreshold, pointsThreshold);

        foreach (var storyDto in storyDtos)
        {
            await _storiesRepo.AddAsync(storyDto);
            yield return storyDto;
        }
    }
}