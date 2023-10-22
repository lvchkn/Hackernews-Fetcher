using Hackernews_Fetcher.Models;

namespace Hackernews_Fetcher.Repos;

public interface ICommentsRepository
{
    Task<List<CommentDto>> GetAllAsync();
    Task AddAsync(CommentDto commentDto);
}