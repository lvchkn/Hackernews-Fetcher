using Microsoft.AspNetCore.Mvc;
using Repos.StoriesRepository;

public static class StoriesController
{
    public static IEndpointRouteBuilder MapStoriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/stories", async ([FromServices] IStoriesRepository storiesRepository) => 
        {
            var stories = await storiesRepository.GetAllAsync();

            return Results.Ok(stories);
        });
        
        app.MapGet("/api/stories/{id}", async (
            [FromRoute] int id,
            [FromServices] IStoriesRepository storiesRepository) => 
        {
            var story = await storiesRepository.GetById(id);

            return Results.Ok(story);
        });
        
        return app;
    }
}