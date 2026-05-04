namespace BunkerAPI.Models.Api;

public sealed class CreateSessionRequest
{
    public string? HostName { get; init; }
}

public sealed class JoinSessionRequest
{
    public required string Name { get; init; }
}
