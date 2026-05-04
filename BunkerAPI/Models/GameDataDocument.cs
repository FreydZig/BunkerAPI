namespace BunkerAPI.Models;

public sealed class GameDataDocument
{
    public List<string> Professions { get; init; } = [];
    public List<string> HealthConditions { get; init; } = [];
    public List<string> Hobbies { get; init; } = [];
    public List<string> LuggageItems { get; init; } = [];
    public List<string> Traits { get; init; } = [];
    public List<string> AdditionalFacts { get; init; } = [];
}
