using CustomerService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controller
{
    [ApiController]
    [Route("api/commerce/orders")]
    public class CommerceOrdersController : ControllerBase
    {
        private readonly IOrderProxyService _orderProxyService;

        public CommerceOrdersController(IOrderProxyService orderProxyService)
        {
            _orderProxyService = orderProxyService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] object payload)
        {
            var result = await _orderProxyService.CreateOrderAsync(payload);
            return Content(result, "application/json");
        }
    }
}