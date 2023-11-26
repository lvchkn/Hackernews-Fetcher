namespace Hackernews_Fetcher.Models;

public record StoryPublishDto : IMessage
{
    public int Id { get; init; }
    public string By { get; init; } = string.Empty;
    public int[] Kids { get; init; } = Array.Empty<int>();
    public int Time { get; init; }
    public int Descendants { get; init; }
    public int Score { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}