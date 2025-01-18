using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Hackernews_Fetcher.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record CommentDto : IMessage
{
    public string Id { get; init; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string By { get; init; } = string.Empty;
    
    [JsonPropertyName("children")]
    public CommentDto[] Kids { get; init; } = [];
    
    [JsonPropertyName("parent_id")]
    public int Parent { get; init; }
    public string Text { get; init; } = string.Empty;
    
    [JsonPropertyName("created_at_i")]
    public int Time { get; init; }
}