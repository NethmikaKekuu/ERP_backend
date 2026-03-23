using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    // ✅ POST: api/orders
    [HttpPost]
    [Authorize] // JWT check
    public IActionResult CreateOrder([FromBody] Order order)
    {
        // TODO: call service later
        return Ok("Order created (temporary)");
    }

    // 🔄 PATCH: api/orders/{id}/status
    [HttpPatch("{id}/status")]
    [Authorize]
    public IActionResult UpdateStatus(int id, [FromBody] string status)
    {
        return Ok($"Order {id} status updated to {status}");
    }

    // ❌ PATCH: api/orders/{id}/cancel
    [HttpPatch("{id}/cancel")]
    [Authorize]
    public IActionResult CancelOrder(int id)
    {
        return Ok($"Order {id} cancelled");
    }
}