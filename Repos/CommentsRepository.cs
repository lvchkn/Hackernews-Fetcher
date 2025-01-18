using Hackernews_Fetcher.Models;
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hackernews_Fetcher.Repos;

public class CommentsRepository : ICommentsRepository
{
    private readonly IMongoCollection<Comment> _commentsCollection;
    private readonly IMapper _mapper;

    public CommentsRepository(IOptions<MongoSettings> mongoSettings, IMapper mapper)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.FeedDatabaseName);
        _commentsCollection = database.GetCollection<Comment>(mongoSettings.Value.CommentsCollectionName);         
        _mapper = mapper;
    }

    public async Task<List<CommentDto>> GetAllAsync()
    {
        var commentsCursor = await _commentsCollection.FindAsync(_ => true);

        var comments = await commentsCursor.ToListAsync();
        var commentDtos = comments.Select(comment => _mapper.Map<CommentDto>(comment)).ToList();

        return commentDtos;
    }

    public async Task AddAsync(CommentDto commentDto)
    {
        var comment = _mapper.Map<Comment>(commentDto);
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, comment.Id);

        await _commentsCollection.ReplaceOneAsync(filter, comment, new ReplaceOptions { IsUpsert = true });
    }
}