using Hackernews_Fetcher.Models;

namespace Hackernews_Fetcher.Repos;

public interface IStoriesRepository
{
    Task<int> GetBiggestIdAsync();
    Task<List<StoryDto>> GetAllAsync();
    Task<StoryDto> GetById(int id);
    Task AddAsync(StoryDto storyDto);
}