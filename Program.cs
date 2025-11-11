using Hackernews_Fetcher;
using Hackernews_Fetcher.Controllers;
using Prometheus;

var builder = WebApplication.CreateBuilder();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDependencies(builder.Configuration);
builder.Services.UseHttpClientMetrics();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapStoriesEndpoints();
app.UseHttpMetrics();
app.MapMetrics();

app.Run();

