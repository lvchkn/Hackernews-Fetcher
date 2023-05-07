using Models;

namespace Repos.CommentsRepository;

public interface ICommentsRepository
{
    Task<List<CommentDto>> GetAllAsync();
    Task AddAsync(CommentDto commentDto);
}