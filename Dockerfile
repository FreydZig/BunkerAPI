# Сборка и публикация ASP.NET Core для Render (Docker Web Service).
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BunkerAPI/BunkerAPI.csproj BunkerAPI/
RUN dotnet restore BunkerAPI/BunkerAPI.csproj

COPY BunkerAPI/ BunkerAPI/
RUN dotnet publish BunkerAPI/BunkerAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "BunkerAPI.dll"]
