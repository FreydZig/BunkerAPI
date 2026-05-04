namespace BunkerAPI.Models;

public sealed class GameSession
{
    public required string SessionCode { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public GamePhase Phase { get; set; }
    public required Guid HostPlayerId { get; init; }
    public List<GamePlayer> Players { get; } = [];
}
