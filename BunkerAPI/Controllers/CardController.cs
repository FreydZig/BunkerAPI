using BunkerAPI.Models;
using BunkerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BunkerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardController(ICardService cardService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Card), StatusCodes.Status200OK)]
    public ActionResult<Card> GetRandomCard() => Ok(cardService.GetRandomCard());
}
