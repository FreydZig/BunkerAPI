namespace BunkerAPI.Models;

public sealed class GamePlayer
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public Card? AssignedCard { get; set; }
}
