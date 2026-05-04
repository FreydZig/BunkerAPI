using BunkerAPI.Models.Api;

namespace BunkerAPI.Services;

public interface IGameSessionService
{
    bool TryCreateSession(CreateSessionRequest? request, out CreateSessionResponse? response, out string? error);
    bool TryJoinSession(Guid sessionId, JoinSessionRequest request, out JoinSessionResponse? response, out SessionJoinFailure failure);
    bool TryStartGame(Guid sessionId, Guid actorPlayerId, out string? error);
    SessionViewDto? GetSessionView(Guid sessionId, Guid? viewerPlayerId);
}
