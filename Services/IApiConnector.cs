using Hackernews_Fetcher.Models;

namespace Hackernews_Fetcher.Services;

public interface IApiConnector
{
    IAsyncEnumerable<StoryDto?> GetNewStories();
    IAsyncEnumerable<StoryDto?> GetTopStories();
}