using BunkerAPI.Models.Api;

namespace BunkerAPI.Services;

public interface IGameSessionService
{
    bool TryCreateSession(CreateSessionRequest? request, out CreateSessionResponse? response, out string? error);
    bool TryJoinSession(string sessionCode, JoinSessionRequest request, out JoinSessionResponse? response, out SessionJoinFailure failure);
    bool TryStartGame(string sessionCode, Guid actorPlayerId, out string? error);
    SessionViewDto? GetSessionView(string sessionCode, Guid? viewerPlayerId);
}
