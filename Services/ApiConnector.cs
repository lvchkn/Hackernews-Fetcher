using System.Text.Json;
using Models;
using Utils;

namespace Services;

public class ApiConnector : IApiConnector
{
    private readonly ILogger<ApiConnector> _logger;
    private readonly HttpClient _client;
    private const string ApiUrlItem = "item/{0}.json";
    private const string ApiUrlTopStories = "newstories.json";
    private const string ApiUrlMaxItem = "maxitem.json";
    private const string CommentType = "comment";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiConnector(ILogger<ApiConnector> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _client = clientFactory.CreateClient("ApiV0");
    }

    public async Task<int[]> GetTopStoryIds(CancellationToken token)
    {
        var response = await _client.GetAsync(ApiUrlTopStories);
        var responseString = await response.Content.ReadAsStringAsync();

        var storyIds = JsonSerializer.Deserialize<int[]>(responseString, _jsonSerializerOptions) ?? new int[0];

        return storyIds;
    }

    public async IAsyncEnumerable<Story?> GetTopStories()
    {
        var token = new CancellationTokenSource().Token;
        var storyIds = await GetTopStoryIds(token);

        if (storyIds.Length == 0)
        {
            yield return null;
        }

        var lastSavedId = 33365948;
        var newIds = storyIds.Where(id => id > lastSavedId).ToList();

        foreach (var id in newIds)
        {
            var response = await _client.GetAsync(string.Format(ApiUrlItem, id));
            var responseString = await response.Content.ReadAsStringAsync();
            var story = JsonSerializer.Deserialize<Story>(responseString, _jsonSerializerOptions);

            if (story is null) continue;

            yield return story;
        }
    }

    public async Task<Comment?> GetComment(int id)
    {
        var response = await _client.GetAsync(string.Format(ApiUrlItem, id.ToString()));
        var responseString = await response.Content.ReadAsStringAsync();

        var itemType = ItemUtils.GetItemType(responseString);

        if (itemType?.ToLower() != CommentType)
        {
            return null;
        }

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var comment = JsonSerializer.Deserialize<Comment>(responseString, jsonSerializerOptions);

        _logger.Log(LogLevel.Information, "Comment has been received: {0}", comment);

        return comment;
    }

    public async Task<Comment?> GetLastComment(CancellationToken token)
    {
        var response = await _client.GetAsync(ApiUrlMaxItem);
        var responseString = await response.Content.ReadAsStringAsync();

        var id = JsonSerializer.Deserialize<int>(responseString);
        Comment? comment;
        var maxRetries = 100;

        do
        {
            comment = await GetComment(id);
            id--;
            maxRetries--;

        } while (comment is null && id > 1 && maxRetries > 0);

        return comment;
    }
}