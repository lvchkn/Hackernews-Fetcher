using Hackernews_Fetcher.Models;

namespace Hackernews_Fetcher.Repos;

public interface IStoriesRepository
{
    Task<long> GetLatestTimestampAsync();
    Task<List<StoryHnDto>> GetAllAsync(int limit = 500);
    Task<StoryHnDto> GetByIdAsync(int id);
    Task AddAsync(StoryHnDto storyHnDto);
}