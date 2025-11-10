using Hackernews_Fetcher.Models;
using JetBrains.Annotations;
using Riok.Mapperly.Abstractions;

namespace Hackernews_Fetcher.Utils;

[UsedImplicitly]
[Mapper]
public partial class Mapper
{
    [MapperIgnoreSource(nameof(StoryHnDto.Comments))]
    public partial StoryPublishDto StoryHnDtoToStoryPublishDto(StoryHnDto storyHnDto);
    
    [MapperIgnoreTarget(nameof(StoryHnDto.Comments))]
    public partial StoryHnDto StoryPublishDtoToStoryHnDto(StoryPublishDto storyHnDto);

    [MapProperty(nameof(Story.Id), nameof(StoryHnDto.Id), Use = nameof(MapStringIdToInt))]
    public partial StoryHnDto StoryToStoryHnDto(Story story);
    
    [MapProperty(nameof(StoryHnDto.Id), nameof(Story.Id), Use = nameof(MapIntIdToString))]
    public partial Story StoryHnDtoToStory(StoryHnDto storyHnDto);
    
    [MapperIgnoreTarget(nameof(Comment.Type))]
    public partial Comment CommentDtoToComment(CommentDto commentDto);
    
    public partial List<Comment> CommentDtoListToCommentList(List<CommentDto> commentDto);
    public partial List<CommentDto> CommentListToCommentDtoList(List<Comment> commentDto);

    [MapperIgnoreSource(nameof(Comment.Type))]
    public partial CommentDto CommentToCommentDto(Comment comment);
    
    [UserMapping(Default = false)]
    private string MapIntIdToString(int id) => id.ToString();
    
    [UserMapping(Default = false)]
    private int MapStringIdToInt(string id) => int.Parse(id);
}