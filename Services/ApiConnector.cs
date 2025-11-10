using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Repos;
using Hackernews_Fetcher.Utils;

namespace Hackernews_Fetcher.Services;

public class ApiConnector : IApiConnector, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStoriesRepository _storiesRepo;
    private readonly Mapper _mapper;
    private readonly ILogger<ApiConnector> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    
    private const string HttpClientName = "ApiV0";
    private readonly Uri _baseAddress;
    private const int MaxConcurrentRequests = 4;
    private readonly SemaphoreSlim _semaphore = new(MaxConcurrentRequests, MaxConcurrentRequests);

    public ApiConnector(IHttpClientFactory clientFactory,
        IStoriesRepository storiesRepo, 
        Mapper mapper, 
        ILogger<ApiConnector> logger)
    {
        _httpClientFactory = clientFactory;
        _baseAddress = clientFactory.CreateClient(HttpClientName).BaseAddress ?? throw new ArgumentException("API Base Address is not provided");
        _storiesRepo = storiesRepo;
        _mapper = mapper;
        _logger = logger;
    }

    private async Task<T> GetApiResponse<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResponseObject = JsonSerializer.Deserialize<T>(responseString, _jsonSerializerOptions);

        return apiResponseObject ?? throw new SerializationException("Could not deserialize the API response.");
    }
    
    private async Task<List<CommentDto>> GetCommentsForStory(StoryHnDto storyDto, CancellationToken cancellationToken)
    {
        var comments = new ConcurrentBag<CommentDto>();

        var tasks = storyDto.Kids.Select(async commentId => 
        { 
            await _semaphore.WaitAsync(cancellationToken);
            
            try
            {
                var requestUri = $"items/{commentId}";
                var comment = await GetApiResponse<CommentDto>(requestUri, cancellationToken);
                comments.Add(comment);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to load comment {CommentId} for story {StoryId}", commentId, storyDto.Id);
            }
            finally
            {
                _semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);

        return comments.ToList();
    }

    private async Task<List<StoryHnDto>> GetStoriesAsync(string query = "", CancellationToken cancellationToken = default)
    {
        const string tags = "story";
        const int hitsPerPage = 200;
        const int startingPage = 0;
        const int pointsThreshold = 3;
        const int maxPagesToFetch = 3;
        var timeThreshold = await _storiesRepo.GetLatestTimestampAsync();
        var uriQuery = Uri.EscapeDataString(query);
        
        var search = "search?query={0}" +
                   "&tags={1}" +
                   "&hitsPerPage={2}" +
                   "&page={3}" +
                   "&numericFilters=points>{4}," +
                   "created_at_i>{5}";

        var searchQuery = string.Format(search, uriQuery, tags, hitsPerPage, startingPage, pointsThreshold, timeThreshold);
        
        _logger.LogInformation("Searching URL: {BaseAddress}{SearchQuery}", _baseAddress, searchQuery);
        var apiResponse = await GetApiResponse<ApiResponse<StoryHnDto>>(searchQuery, cancellationToken);
        _logger.LogInformation("Fetched about {NumberOfStories} new stories", apiResponse.Data.Length * apiResponse.NumberOfPages);
        
        if (apiResponse.Data.Length == 0)
        {
            return [];
        }

        var stories = new List<StoryHnDto>(apiResponse.Data.Length);
        
        if (apiResponse.NumberOfPages < 2)
        {
            return apiResponse.Data.ToList();
        }

        var numberOfPagesLimit = Math.Min(apiResponse.NumberOfPages, maxPagesToFetch);
        
        for (var i = 1; i <= numberOfPagesLimit; i++)
        {
            var requestUri = string.Format(search, uriQuery, tags, hitsPerPage, i, pointsThreshold, timeThreshold);
            var nextPageResponse = await GetApiResponse<ApiResponse<StoryHnDto>>(requestUri, cancellationToken);
            stories.AddRange(nextPageResponse.Data);
        }
        
        return stories;
    }

    public async IAsyncEnumerable<StoryHnDto> GetNewStoriesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var storyDtos = await GetStoriesAsync(cancellationToken: cancellationToken);

        foreach (var storyDto in storyDtos)
        {
            try
            {
                var commentsDto = await GetCommentsForStory(storyDto, cancellationToken);
                var storyWithComments = storyDto with { Comments = _mapper.CommentDtoListToCommentList(commentsDto) };
                await _storiesRepo.AddAsync(storyWithComments);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to add story {StoryId}: {ErrorMessage}. Continuing...", storyDto.Id, ex.Message);
                continue;
            }
            
            yield return storyDto;
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}