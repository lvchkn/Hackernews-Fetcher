using System.Text.Json.Serialization;

namespace Hackernews_Fetcher.Models;

public record StoryHnDto
{
    [JsonPropertyName("story_id")]
    public int Id { get; init; }

    [JsonPropertyName("author")] 
    public string By { get; init; } = string.Empty;

    [JsonPropertyName("children")] 
    public int[] Kids { get; init; } = Array.Empty<int>();
    
    [JsonPropertyName("created_at_i")]
    public int Time { get; init; }
    
    [JsonPropertyName("num_comments")]
    public int Descendants { get; init; }
    
    [JsonPropertyName("points")]
    public int Score { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("story_text")] 
    public string Text { get; init; } = string.Empty;
    
    [JsonPropertyName("url")] 
    public string Url { get; init; } = string.Empty;
}