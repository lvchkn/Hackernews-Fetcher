namespace Hackernews_Fetcher.Models;

public record Story
{
    public string Id { get; init; } = string.Empty;
    public string By { get; init; } = string.Empty;
    public int Descendants { get; init; }
    public int[] Kids { get; init; } = Array.Empty<int>();
    public int Score { get; init; }
    public int Time { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}