using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Hackernews_Fetcher.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record ApiResponse<T> where T: IMessage
{
    [JsonPropertyName("hits")] 
    public T[] Data { get; init; } = [];
    
    [JsonPropertyName("hitsPerPage")]
    public int PageSize { get; init; }
    
    [JsonPropertyName("nbHits")]
    public int TotalStoriesCount { get; init; }
    
    [JsonPropertyName("nbPages")]
    public int NumberOfPages { get; init; }
    
    [JsonPropertyName("page")]
    public int CurrentPage { get; init; }
}