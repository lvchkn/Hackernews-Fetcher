namespace Models;

public record MongoSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string FeedDatabaseName { get; init; } = string.Empty;
    public string CommentsCollectionName { get; init; } = string.Empty;
    public string StoriesCollectionName { get; init; } = string.Empty;
}