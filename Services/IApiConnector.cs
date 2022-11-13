using Models;

namespace Services;

public interface IApiConnector
{
    Task<Comment?> GetComment(int id);
    Task<Comment?> GetLastComment(CancellationToken token);

    Task<int[]> GetTopStoryIds(CancellationToken token);
    IAsyncEnumerable<Story?>GetTopStories();
}