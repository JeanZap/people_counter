FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PeopleCounter.slnx .
COPY src ./src
RUN dotnet restore src/PeopleCounter.WebApi/PeopleCounter.WebApi.csproj
RUN dotnet publish src/PeopleCounter.WebApi/PeopleCounter.WebApi.csproj -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PeopleCounter.WebApi.dll"]
