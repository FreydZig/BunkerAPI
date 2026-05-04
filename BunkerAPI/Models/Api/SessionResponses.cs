using BunkerAPI.Models;

namespace BunkerAPI.Models.Api;

public sealed class CreateSessionResponse
{
    public required string SessionId { get; init; }
    public required Guid PlayerId { get; init; }
    public required Guid HostPlayerId { get; init; }
}

public sealed class JoinSessionResponse
{
    public required Guid PlayerId { get; init; }
}

public sealed class PlayerViewDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public Card? Card { get; init; }
}

public sealed class SessionViewDto
{
    public required string SessionId { get; init; }
    public required GamePhase Phase { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required Guid HostPlayerId { get; init; }
    public required IReadOnlyList<PlayerViewDto> Players { get; init; }
}
