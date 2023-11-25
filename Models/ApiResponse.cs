using System.Text.Json.Serialization;

namespace Hackernews_Fetcher.Models;

public record ApiResponse
{
    [JsonPropertyName("hits")] 
    public StoryHnDto[] Stories { get; init; } = Array.Empty<StoryHnDto>();
    
    [JsonPropertyName("hitsPerPage")]
    public int PageSize { get; init; }
    
    [JsonPropertyName("nbHits")]
    public int TotalStoriesCount { get; init; }
    
    [JsonPropertyName("nbPages")]
    public int NumberOfPages { get; init; }
    
    [JsonPropertyName("page")]
    public int CurrentPage { get; init; }
}