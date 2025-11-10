using Hackernews_Fetcher.Models;
using Hackernews_Fetcher.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hackernews_Fetcher.Repos;

public class StoriesRepository : IStoriesRepository
{
    private readonly IMongoCollection<Story> _storiesCollection;
    private readonly Mapper _mapper;

    public StoriesRepository(IOptions<MongoSettings> mongoSettings, Mapper mapper)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.FeedDatabaseName);
        _storiesCollection = database.GetCollection<Story>(mongoSettings.Value.StoriesCollectionName);         
        _mapper = mapper;
    }

    public async Task<long> GetLatestTimestampAsync()
    {
        var sortCondition = Builders<Story>.Sort.Descending(c => c.Time);

        var latestStory = await _storiesCollection
            .Find(_ => true)
            .Sort(sortCondition)
            .Limit(1)
            .FirstOrDefaultAsync();

        var timestamp = latestStory?.Time ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return timestamp;
    }

    public async Task<List<StoryHnDto>> GetAllAsync(int limit = 500)
    {
        var storiesCursor = await _storiesCollection.Find(_ => true).Limit(limit).ToCursorAsync();

        var stories = await storiesCursor.ToListAsync();
        var storyDtos = stories.Select(story => _mapper.StoryToStoryHnDto(story))
            .ToList();

        return storyDtos;
    }

    public async Task<StoryHnDto> GetByIdAsync(int id)
    {
        var filter = Builders<Story>.Filter.Eq(story => story.Id, id.ToString());
        var storiesCursor = await _storiesCollection.FindAsync(filter);

        var story = await storiesCursor.FirstOrDefaultAsync();
        var storyDto = _mapper.StoryToStoryHnDto(story);

        return storyDto;
    }

    public async Task AddAsync(StoryHnDto storyHnDto)
    {
        var story = _mapper.StoryHnDtoToStory(storyHnDto);
        var filter = Builders<Story>.Filter.Eq(s => s.Id, story.Id);

        await _storiesCollection.ReplaceOneAsync(filter, story, new ReplaceOptions { IsUpsert = true });
    }
}