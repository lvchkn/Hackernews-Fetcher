using Hackernews_Fetcher;
using Hackernews_Fetcher.Controllers;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder();
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddSerilog(configuration =>
    {
        configuration.ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
    });
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

    app.UseSerilogRequestLogging();
    app.MapStoriesEndpoints();
    app.UseHttpMetrics();
    app.MapMetrics();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fatal error occured while running the application");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

