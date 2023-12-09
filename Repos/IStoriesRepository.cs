using Hackernews_Fetcher.Models;

namespace Hackernews_Fetcher.Repos;

public interface IStoriesRepository
{
    Task<int> GetLatestTimestampAsync();
    Task<List<StoryHnDto>> GetAllAsync();
    Task<StoryHnDto> GetByIdAsync(int id);
    Task AddAsync(StoryHnDto storyHnDto);
}