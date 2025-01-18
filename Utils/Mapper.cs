using Hackernews_Fetcher.Models;
using AutoMapper;
using JetBrains.Annotations;

namespace Hackernews_Fetcher.Utils;

[UsedImplicitly]
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Story, StoryHnDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.Parse(src.Id)));
        CreateMap<StoryHnDto, Story>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        CreateMap<StoryHnDto, StoryPublishDto>().ReverseMap();

        CreateMap<Comment, CommentDto>().ReverseMap();
    }
}