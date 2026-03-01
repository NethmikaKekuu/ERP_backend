using Microsoft.AspNetCore.Mvc;

namespace PredictionService.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
        => Ok("PredictionService is running.");
}