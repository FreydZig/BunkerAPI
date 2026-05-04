using System.Text.Json;
using BunkerAPI.Models;

namespace BunkerAPI.Services;

public sealed class CardService : ICardService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly GameDataDocument _data;

    public CardService(IWebHostEnvironment environment)
    {
        var path = Path.Combine(environment.ContentRootPath, "Data", "game-data.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Не найден файл данных: {path}", path);

        var json = File.ReadAllText(path);
        _data = JsonSerializer.Deserialize<GameDataDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Не удалось разобрать game-data.json.");
    }

    public Card GetRandomCard() =>
        new()
        {
            Profession = Pick(_data.Professions),
            HealthCondition = Pick(_data.HealthConditions),
            Hobby = Pick(_data.Hobbies),
            LuggageItem = Pick(_data.LuggageItems),
            Trait = Pick(_data.Traits),
            AdditionalFact = Pick(_data.AdditionalFacts),
        };

    private static string Pick(IReadOnlyList<string> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Пустой список в game-data.json.");

        return items[Random.Shared.Next(items.Count)];
    }
}
