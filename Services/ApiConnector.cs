using System.Runtime.Serialization;
using System.Text.Json;
using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Repos;
using AutoMapper;

namespace Hackernews_Fetcher.Services;

public class ApiConnector : IApiConnector
{
    private readonly HttpClient _httpClient;
    private readonly IStoriesRepository _storiesRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ApiConnector> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ApiConnector(IHttpClientFactory clientFactory,
        IStoriesRepository storiesRepo, 
        IMapper mapper, 
        ILogger<ApiConnector> logger)
    {
        _httpClient = clientFactory.CreateClient("ApiV0");
        _storiesRepo = storiesRepo;
        _mapper = mapper;
        _logger = logger;
    }

    private async Task<T> GetApiResponse<T>(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        var responseString = await response.Content.ReadAsStringAsync();

        var apiResponseObject = JsonSerializer.Deserialize<T>(responseString, _jsonSerializerOptions);

        return apiResponseObject ?? throw new SerializationException("Could not deserialize the API response.");
    }
    
    private async Task<List<CommentDto>> GetCommentsForStory(StoryHnDto storyDto)
    {
        var comments = new List<CommentDto>();

        foreach (var commentId in storyDto.Kids)
        {
            var requestUri = $"items/{commentId}";
            var comment = await GetApiResponse<CommentDto>(requestUri);
            comments.Add(comment);
        }

        return comments;
    }

    private async Task<List<StoryHnDto>> GetStoriesAsync(string query = "")
    {
        const string tags = "story";
        const int hitsPerPage = 200;
        const int startingPage = 0;
        const int pointsThreshold = 3;
        var timeThreshold = await _storiesRepo.GetLatestTimestampAsync();
        
        var search = "search?query={0}" +
                   "&tags={1}" +
                   "&hitsPerPage={2}" +
                   "&page={3}" +
                   "&numericFilters=points>{4}," +
                   "created_at_i>{5}";

        var searchQuery = string.Format(search, query, tags, hitsPerPage, startingPage, pointsThreshold, timeThreshold);
        _logger.LogInformation($"Searching URL: {_httpClient.BaseAddress}{searchQuery}");
        
        var apiResponse = await GetApiResponse<ApiResponse<StoryHnDto>>(searchQuery);
        _logger.LogInformation($"Fetched {apiResponse.Data.Length * apiResponse.NumberOfPages} new stories");
        
        var stories = new List<StoryHnDto>(apiResponse.Data.Length);
        
        if (apiResponse.Data.Length == 0)
        {
            return stories;
        }

        if (apiResponse.NumberOfPages < 2)
        {
            return apiResponse.Data.ToList();
        }

        var numberOfPagesLimit = Math.Min(apiResponse.NumberOfPages, 3);
        
        for (var i = 1; i <= numberOfPagesLimit; i++)
        {
            var nextPageResponse = await GetApiResponse<ApiResponse<StoryHnDto>>(string.Format(search, query, tags, hitsPerPage, i, pointsThreshold, timeThreshold));
            stories.AddRange(nextPageResponse.Data);
        }
        
        return stories;
    }

    public async IAsyncEnumerable<StoryHnDto?> GetNewStoriesAsync()
    {
        var storyDtos = await GetStoriesAsync();

        foreach (var storyDto in storyDtos)
        {
            var commentsDto = await GetCommentsForStory(storyDto);
            var storyWithComments = storyDto with { Comments = _mapper.Map<List<Comment>>(commentsDto) };
            await _storiesRepo.AddAsync(storyWithComments);
            yield return storyDto;
        }
    }
}