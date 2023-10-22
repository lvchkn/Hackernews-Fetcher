using Hackernews_Fetcher.Models;
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

    public async Task<int> GetBiggestIdAsync()
    {
        var sortCondition = Builders<Story>.Sort.Descending(c => c.Id);

        var latestStory = await _storiesCollection
            .Find(_ => true)
            .Sort(sortCondition)
            .Limit(1)
            .FirstOrDefaultAsync();

        string id = latestStory?.Id 
            ?? throw new NullReferenceException("Could not retrieve the document with the biggest id");

        bool idIsNumber = int.TryParse(id, out int idNumber);

        if (!idIsNumber) throw new InvalidCastException("Retrieved story's Id is not a number");

        return idNumber;
    }

    public async Task<List<StoryDto>> GetAllAsync()
    {
        var storiesCursor = await _storiesCollection.FindAsync(_ => true);

        var stories = await storiesCursor.ToListAsync();
        var storyDtos = stories.Select(MapToStoryDto).ToList();

        return storyDtos;
    }

    public async Task<StoryDto> GetById(int id)
    {
        var filter = Builders<Story>.Filter.Eq(story => story.Id, id.ToString());
        var storiesCursor = await _storiesCollection.FindAsync(filter);

        var story = await storiesCursor.FirstOrDefaultAsync();
        var storyDto = MapToStoryDto(story);

        return storyDto;
    }

    public async Task AddAsync(StoryDto storyDto)
    {
        var story = MapToStory(storyDto);
        var filter = Builders<Story>.Filter.Eq(s => s.Id, story.Id);

        await _storiesCollection.ReplaceOneAsync(filter, story, new ReplaceOptions { IsUpsert = true });
    }

    private static Story MapToStory(StoryDto storyDto)
    {
        return new Story
        {
            Id = storyDto.Id.ToString(),
            By = storyDto.By,
            Descendants = storyDto.Descendants,
            Kids = storyDto.Kids,
            Score = storyDto.Score,
            Time = storyDto.Time,
            Title = storyDto.Title,
            Url = storyDto.Url,
            Type = storyDto.Type,
        };
    }

    private static StoryDto MapToStoryDto(Story story)
    {
        return new StoryDto
        {
            Id = int.Parse(story.Id),
            By = story.By,
            Descendants = story.Descendants,
            Kids = story.Kids,
            Score = story.Score,
            Time = story.Time,
            Title = story.Title,
            Url = story.Url,
            Type = story.Type,
        };
    }
}