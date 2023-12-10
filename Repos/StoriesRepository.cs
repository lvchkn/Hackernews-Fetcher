using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hackernews_Fetcher.Repos;

public class StoriesRepository : IStoriesRepository
{
   private readonly IMongoCollection<Story> _storiesCollection;

    public StoriesRepository(IOptions<MongoSettings> mongoSettings)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.FeedDatabaseName);
        _storiesCollection = database.GetCollection<Story>(mongoSettings.Value.StoriesCollectionName);         
    }

    public async Task<int> GetLatestTimestampAsync()
    {
        var sortCondition = Builders<Story>.Sort.Descending(c => c.Time);

        var latestStory = await _storiesCollection
            .Find(_ => true)
            .Sort(sortCondition)
            .Limit(1)
            .FirstOrDefaultAsync();

        var timestamp = latestStory?.Time ?? 0;

        return timestamp;
    }

    public async Task<List<StoryHnDto>> GetAllAsync()
    {
        var storiesCursor = await _storiesCollection.FindAsync(_ => true);

        var stories = await storiesCursor.ToListAsync();
        var storyDtos = stories.Select(s => s.MapToStoryDto())
            .ToList();

        return storyDtos;
    }

    public async Task<StoryHnDto> GetByIdAsync(int id)
    {
        var filter = Builders<Story>.Filter.Eq(story => story.Id, id.ToString());
        var storiesCursor = await _storiesCollection.FindAsync(filter);

        var story = await storiesCursor.FirstOrDefaultAsync();
        var storyDto = story.MapToStoryDto();

        return storyDto;
    }

    public async Task AddAsync(StoryHnDto storyHnDto)
    {
        var story = storyHnDto.MapToStory();
        var filter = Builders<Story>.Filter.Eq(s => s.Id, story.Id);

        await _storiesCollection.ReplaceOneAsync(filter, story, new ReplaceOptions { IsUpsert = true });
    }
}