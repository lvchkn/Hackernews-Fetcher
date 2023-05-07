using Microsoft.Extensions.Options;
using Models;
using MongoDB.Driver;

namespace Repos.CommentsRepository;

public class CommentsRepository : ICommentsRepository
{
    private readonly IMongoCollection<Comment> _commentsCollection;

    public CommentsRepository(IOptions<MongoSettings> mongoSettings)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.FeedDatabaseName);
        _commentsCollection = database.GetCollection<Comment>(mongoSettings.Value.CommentsCollectionName);         
    }

    public async Task<List<CommentDto>> GetAllAsync()
    {
        var commentsCursor = await _commentsCollection.FindAsync(_ => true);

        var comments = await commentsCursor.ToListAsync();
        var commentDtos = comments.Select(comment => MapToCommentDto(comment)).ToList();

        return commentDtos;
    }

    public async Task AddAsync(CommentDto commentDto)
    {
        var comment = MapToComment(commentDto);
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, comment.Id);

        await _commentsCollection.ReplaceOneAsync(filter, comment, new ReplaceOptions { IsUpsert = true });
    }

    private Comment MapToComment(CommentDto commentDto)
    {
        return new Comment
        {
            Id = commentDto.Id.ToString(),
            By = commentDto.By,
            Kids = (int[]) commentDto.Kids.Clone(),
            Parent = commentDto.Parent,
            Time = commentDto.Time,
            Text = commentDto.Text,
            Type = commentDto.Type,
        };
    }

    private CommentDto MapToCommentDto(Comment comment)
    {
        return new CommentDto
        {
            Id = int.Parse(comment.Id),
            By = comment.By,
            Kids = (int[]) comment.Kids.Clone(),
            Parent = comment.Parent,
            Time = comment.Time,
            Text = comment.Text,
            Type = comment.Type,
        };
    }
}