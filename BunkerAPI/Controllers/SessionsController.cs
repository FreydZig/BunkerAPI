using BunkerAPI.Models.Api;
using BunkerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BunkerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController(IGameSessionService sessions) : ControllerBase
{
    public const string PlayerIdHeader = "X-Player-Id";

    [HttpPost]
    [ProducesResponseType(typeof(CreateSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CreateSessionResponse> CreateSession([FromBody] CreateSessionRequest? request)
    {
        if (!sessions.TryCreateSession(request, out var created, out var error) || created is null)
            return BadRequest(error);

        return CreatedAtAction(nameof(GetSession), new { sessionId = created.SessionId }, created);
    }

    [HttpPost("{sessionId:guid}/players")]
    [ProducesResponseType(typeof(JoinSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<JoinSessionResponse> JoinSession(Guid sessionId, [FromBody] JoinSessionRequest request)
    {
        if (!sessions.TryJoinSession(sessionId, request, out var response, out var failure))
            return JoinFailureResult(failure);

        return Ok(response);
    }

    [HttpPost("{sessionId:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult StartGame(Guid sessionId, [FromHeader(Name = PlayerIdHeader)] Guid? playerId)
    {
        if (playerId is null || playerId == Guid.Empty)
            return Problem(statusCode: 400, title: "Заголовок X-Player-Id обязателен для старта игры.");

        if (!sessions.TryStartGame(sessionId, playerId.Value, out var error))
        {
            if (string.Equals(error, "Сессия не найдена.", StringComparison.Ordinal))
                return NotFound();

            if (string.Equals(error, "Начать игру может только хост.", StringComparison.Ordinal))
                return Problem(statusCode: 403, title: error);

            return Problem(statusCode: 409, title: error);
        }

        return NoContent();
    }

    [HttpGet("{sessionId:guid}", Name = nameof(GetSession))]
    [ProducesResponseType(typeof(SessionViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SessionViewDto> GetSession(
        Guid sessionId,
        [FromHeader(Name = PlayerIdHeader)] Guid? playerId)
    {
        var view = sessions.GetSessionView(sessionId, playerId);
        if (view is null)
            return NotFound();

        return Ok(view);
    }

    private ActionResult JoinFailureResult(SessionJoinFailure failure) =>
        failure switch
        {
            SessionJoinFailure.SessionNotFound => NotFound(),
            SessionJoinFailure.InvalidName => BadRequest("Укажите непустое имя игрока."),
            SessionJoinFailure.GameAlreadyStarted => Conflict("Игра уже началась, присоединиться нельзя."),
            SessionJoinFailure.SessionFull => Conflict("В сессии нет свободных мест."),
            SessionJoinFailure.DuplicateName => Conflict("Игрок с таким именем уже в сессии."),
            _ => BadRequest(),
        };
}
