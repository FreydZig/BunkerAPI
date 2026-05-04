namespace BunkerAPI.Models;

public sealed class Card
{
    public required string Profession { get; init; }
    public required string HealthCondition { get; init; }
    public required string Hobby { get; init; }
    public required string LuggageItem { get; init; }
    public required string Trait { get; init; }
    public required string AdditionalFact { get; init; }
}
