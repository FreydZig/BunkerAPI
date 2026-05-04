namespace BunkerAPI.Models;

public sealed class GameSession
{
    public required Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public GamePhase Phase { get; set; }
    public required Guid HostPlayerId { get; init; }
    public List<GamePlayer> Players { get; } = [];
}
