using System.Collections.Concurrent;
using BunkerAPI.Models;
using BunkerAPI.Models.Api;

namespace BunkerAPI.Services;

public sealed class GameSessionService(ICardService cardService) : IGameSessionService
{
    private const int MaxPlayerNameLength = 64;
    private const int MaxPlayersPerSession = 24;

    private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

    public CreateSessionResponse CreateSession(CreateSessionRequest? request)
    {
        var sessionId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var hostName = NormalizeName(request?.HostName, defaultIfEmpty: "Хост") ?? "Хост";

        var session = new GameSession
        {
            Id = sessionId,
            CreatedAt = DateTimeOffset.UtcNow,
            Phase = GamePhase.Lobby,
            HostPlayerId = hostId,
        };
        session.Players.Add(new GamePlayer { Id = hostId, Name = hostName });

        _sessions[sessionId] = session;

        return new CreateSessionResponse
        {
            SessionId = sessionId,
            PlayerId = hostId,
            HostPlayerId = hostId,
        };
    }

    public bool TryJoinSession(Guid sessionId, JoinSessionRequest request, out JoinSessionResponse? response, out SessionJoinFailure failure)
    {
        response = null;
        failure = SessionJoinFailure.None;

        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            failure = SessionJoinFailure.SessionNotFound;
            return false;
        }

        var name = NormalizeName(request.Name, defaultIfEmpty: null);
        if (name is null)
        {
            failure = SessionJoinFailure.InvalidName;
            return false;
        }

        lock (session)
        {
            if (session.Phase != GamePhase.Lobby)
            {
                failure = SessionJoinFailure.GameAlreadyStarted;
                return false;
            }

            if (session.Players.Count >= MaxPlayersPerSession)
            {
                failure = SessionJoinFailure.SessionFull;
                return false;
            }

            if (session.Players.Exists(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                failure = SessionJoinFailure.DuplicateName;
                return false;
            }

            var playerId = Guid.NewGuid();
            session.Players.Add(new GamePlayer { Id = playerId, Name = name });
            response = new JoinSessionResponse { PlayerId = playerId };
            return true;
        }
    }

    public bool TryStartGame(Guid sessionId, Guid actorPlayerId, out string? error)
    {
        error = null;

        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            error = "Сессия не найдена.";
            return false;
        }

        lock (session)
        {
            if (session.HostPlayerId != actorPlayerId)
            {
                error = "Начать игру может только хост.";
                return false;
            }

            if (session.Phase != GamePhase.Lobby)
            {
                error = "Игра уже запущена или завершена.";
                return false;
            }

            foreach (var player in session.Players)
                player.AssignedCard = cardService.GetRandomCard();

            session.Phase = GamePhase.InProgress;
            return true;
        }
    }

    public SessionViewDto? GetSessionView(Guid sessionId, Guid? viewerPlayerId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return null;

        lock (session)
        {
            var players = new List<PlayerViewDto>(session.Players.Count);
            foreach (var p in session.Players)
            {
                Card? visibleCard = session.Phase == GamePhase.InProgress
                    && viewerPlayerId.HasValue
                    && viewerPlayerId.Value == p.Id
                    ? p.AssignedCard
                    : null;

                players.Add(new PlayerViewDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Card = visibleCard,
                });
            }

            return new SessionViewDto
            {
                SessionId = session.Id,
                Phase = session.Phase,
                CreatedAt = session.CreatedAt,
                HostPlayerId = session.HostPlayerId,
                Players = players,
            };
        }
    }

    private static string? NormalizeName(string? raw, string? defaultIfEmpty)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultIfEmpty;

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxPlayerNameLength)
            trimmed = trimmed[..MaxPlayerNameLength];

        return trimmed.Length == 0 ? defaultIfEmpty : trimmed;
    }
}
