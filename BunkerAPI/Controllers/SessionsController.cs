using BunkerAPI.Models;
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

    [HttpPost("{sessionId}/players")]
    [ProducesResponseType(typeof(JoinSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<JoinSessionResponse> JoinSession(string sessionId, [FromBody] JoinSessionRequest request)
    {
        if (!SessionCode.TryNormalize(sessionId, out var code))
            return BadRequest("Код комнаты: ровно 6 символов (латиница A–Z и цифры).");

        if (!sessions.TryJoinSession(code, request, out var response, out var failure))
            return JoinFailureResult(failure);

        return Ok(response);
    }

    [HttpPost("{sessionId}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult StartGame(string sessionId, [FromHeader(Name = PlayerIdHeader)] Guid? playerId)
    {
        if (!SessionCode.TryNormalize(sessionId, out var code))
            return BadRequest("Код комнаты: ровно 6 символов (латиница A–Z и цифры).");

        if (playerId is null || playerId == Guid.Empty)
            return Problem(statusCode: 400, title: "Заголовок X-Player-Id обязателен для старта игры.");

        if (!sessions.TryStartGame(code, playerId.Value, out var error))
        {
            if (string.Equals(error, "Сессия не найдена.", StringComparison.Ordinal))
                return NotFound();

            if (string.Equals(error, "Начать игру может только хост.", StringComparison.Ordinal))
                return Problem(statusCode: 403, title: error);

            return Problem(statusCode: 409, title: error);
        }

        return NoContent();
    }

    [HttpGet("{sessionId}", Name = nameof(GetSession))]
    [ProducesResponseType(typeof(SessionViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SessionViewDto> GetSession(
        string sessionId,
        [FromHeader(Name = PlayerIdHeader)] Guid? playerId)
    {
        if (!SessionCode.TryNormalize(sessionId, out var code))
            return BadRequest("Код комнаты: ровно 6 символов (латиница A–Z и цифры).");

        var view = sessions.GetSessionView(code, playerId);
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
