using JetBrains.Annotations;

namespace Hackernews_Fetcher.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record Comment
{
    public string Id { get; init; } = string.Empty;
    public string By { get; init; } = string.Empty;
    public Comment[] Kids { get; init; } = [];
    public int Parent { get; init; }
    public string Text { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int Time { get; init; }
}