using Hackernews_Fetcher.Models;

namespace Hackernews_Fetcher.Utils;

public static class Mapper
{
    public static Story MapToStory(this StoryHnDto storyHnDto)
    {
        return new Story
        {
            Id = storyHnDto.Id.ToString(),
            By = storyHnDto.By,
            Kids = storyHnDto.Kids,
            Descendants = storyHnDto.Descendants,
            Score = storyHnDto.Score,
            Time = storyHnDto.Time,
            Title = storyHnDto.Title,
            Url = storyHnDto.Url,
            Text = storyHnDto.Text,
        };
    }
    
    public static StoryHnDto MapToStoryDto(this Story story)
    {
        return new StoryHnDto
        {
            Id = int.Parse(story.Id),
            By = story.By,
            Kids = story.Kids,
            Descendants = story.Descendants,
            Score = story.Score,
            Time = story.Time,
            Title = story.Title,
            Url = story.Url,
            Text = story.Text,
        };
    }
    
    public static StoryPublishDto MapToPublishDto(this StoryHnDto storyHnDto)
    {
        return new StoryPublishDto
        {
            Id = storyHnDto.Id,
            By = storyHnDto.By,
            Kids = storyHnDto.Kids,
            Descendants = storyHnDto.Descendants,
            Score = storyHnDto.Score,
            Time = storyHnDto.Time,
            Title = storyHnDto.Title,
            Url = storyHnDto.Url,
            Text = storyHnDto.Text,
        };
    }
}