using Models;

namespace Repos.StoriesRepository;

public interface IStoriesRepository
{
    Task<int> GetBiggestIdAsync();
    Task<List<StoryDto>> GetAllAsync();
    Task<StoryDto> GetById(int id);
    Task AddAsync(StoryDto storyDto);
}