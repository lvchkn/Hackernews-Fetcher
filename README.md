# Hackernews-Fetcher

Background service that fetches Hacker News stories and comments, persists them to MongoDB and publishes story events to RabbitMQ.

## Overview

Hackernews-Fetcher periodically queries the Hacker News APIs, maps remote DTOs to domain models, stores stories/comments in MongoDB and publishes story messages to a RabbitMQ queue.

Components:

-   Worker: hosted background service that drives fetch/publish (see [`Hackernews_Fetcher.Worker`](Worker.cs)).
-   ApiConnector: handles paging, fetching comments and rate-limited concurrency (see [`Hackernews_Fetcher.Services.ApiConnector.GetNewStoriesAsync`](Services/ApiConnector.cs)).
-   Repositories: persistence layer for stories/comments (see [Repos/](Repos/)).
-   Mapper: DTO → domain mapping implemented in [`Hackernews_Fetcher.Utils.Mapper`](Utils/Mapper.cs).

## Local development

1. Prepare environment
    - Copy secrets into `.env` (example keys in [.env](.env)).
2. Restore & build
    ```sh
    dotnet restore
    dotnet build
    ```
3. Ensure Mongo and RabbitMQ Docker containers are running
4. Run the worker
    ```sh
    dotnet run --project Hackernews-Fetcher.csproj
    ```

## Configuration

Primary configuration lives in [appsettings.json](appsettings.json). Environment variables override settings (used in CI / Docker Compose).
