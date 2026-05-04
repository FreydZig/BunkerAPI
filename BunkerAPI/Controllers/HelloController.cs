using Microsoft.AspNetCore.Mvc;

namespace BunkerAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public string Get() => "Привет мир";
}
