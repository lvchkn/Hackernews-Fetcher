using Models;

namespace Services;

public interface IApiConnector
{
    Task<int[]?> GetTopStoryIds(CancellationToken token);
    IAsyncEnumerable<StoryDto?> GetTopStories();
}