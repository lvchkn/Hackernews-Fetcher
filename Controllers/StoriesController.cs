using Hackernews_Fetcher.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Hackernews_Fetcher.Controllers;

public static class StoriesController
{
    public static IEndpointRouteBuilder MapStoriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/stories", async ([FromServices] IStoriesRepository storiesRepository) => 
        {
            var stories = await storiesRepository.GetAllAsync();

            return Results.Ok(stories);
        });
        
        app.MapGet("/api/stories/{id:int}", async (
            [FromRoute] int id,
            [FromServices] IStoriesRepository storiesRepository) => 
        {
            var story = await storiesRepository.GetByIdAsync(id);

            return Results.Ok(story);
        });
        
        return app;
    }
}