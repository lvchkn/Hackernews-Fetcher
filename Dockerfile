FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5286
ENV ASPNETCORE_ENVIRONMENT="production"
EXPOSE 5286

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Hackernews-Fetcher.csproj", "."]

RUN dotnet restore "Hackernews-Fetcher.csproj"
COPY . .
RUN dotnet build "Hackernews-Fetcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hackernews-Fetcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hackernews-Fetcher.dll"]
